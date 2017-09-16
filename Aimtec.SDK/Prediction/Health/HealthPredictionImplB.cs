namespace Aimtec.SDK.Prediction.Health
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Aimtec.SDK.Damage;
    using Aimtec.SDK.Extensions;
    using Aimtec.SDK.Menu;
    using Aimtec.SDK.Menu.Components;
    using Aimtec.SDK.Menu.Config;
    using Aimtec.SDK.Orbwalking;

    internal class HealthPredictionImplB : IHealthPrediction
    {
        internal Menu Config { get; set; }

        public HealthPredictionImplB()
        {
            Obj_AI_Base.OnProcessAutoAttack += Obj_AI_Base_OnProcessAutoAttack;
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDestroy += GameObject_OnDestroy;
            Game.OnUpdate += this.Game_OnUpdate;
            Obj_AI_Base.OnPerformCast += this.Obj_AI_Base_OnPerformCast;
            SpellBook.OnStopCast += this.SpellBook_OnStopCast;


            this.Config = new Menu("HealthPred", "HealthPrediction")
            {
                new MenuSeperator("Seperator", "Delays are 0 by default"),
                new MenuSlider("ExtraDelayRanged", "Extra Ranged Delay", 0, 0, 150).SetToolTip("Delay for processing minion ranged attacks"),
                new MenuSlider("ExtraDelayMelee", "Extra Melee Attacks Delay", 0, 0, 150).SetToolTip("Delay for processing minion melee attacks")
            };

            AimtecMenu.Instance.Add(this.Config);
            
        }

        private void SpellBook_OnStopCast(Obj_AI_Base sender, SpellBookStopCastEventArgs e)
        {
            if (sender == null || !sender.IsValid)
            {
                return;
            }

            var ob = Attacks.GetOutBoundAttacks(sender.NetworkId);

            if (ob != null)
            {
                //Remove any pending attacks or attacks in progress because the auto attack was cancelled
                if (e.StopAnimation || e.ForceStop || e.DestroyMissile)
                {
                    foreach (var attack in ob)
                    {
                        if (attack.AttackStatus == AutoAttack.AttackState.Completed)
                        {
                            continue;
                        }

                        attack.Completed();

                        var rattack = attack as RangedAttack;

                        if (rattack == null)
                        {
                            continue;
                        }

                        if (e.DestroyMissile)
                        {
                            rattack.MissileDestroyed();
                        }
                    }
                }
            }
        }

        private void Obj_AI_Base_OnPerformCast(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs e)
        {
            if (sender != null && sender.IsValid && sender.IsMelee)
            {
                var ob = Attacks.GetOutBoundAttacks(sender.NetworkId);
                if (ob != null)
                {
                    var attacks = ob.Where(x => x.AttackStatus != AutoAttack.AttackState.Completed);

                    //Get the oldest attack
                    var attack = attacks.MinBy(x => x.DetectTime);

                    if (attack != null)
                    {
                        attack.AttackStatus = AutoAttack.AttackState.Completed;
                    }
                }
            }
        }

        /// <summary>
        ///     Returns if the name is an auto attack
        /// </summary>
        /// <param name="name">Name of spell</param>
        /// <returns>The <see cref="bool" /></returns>
        public static bool IsAutoAttack(string name)
        {
            name = name.ToLower();
            return name.Contains("attack") && !AOrbwalker.NoAttacks.Contains(name) || AOrbwalker.SpecialAttacks.Contains(name);
        }

        public float GetPrediction(Obj_AI_Base target, int time)
        {
            float predictedDamage = 0;
            foreach (var m in Attacks.OutBoundAttacks)
            {
                var attacks = m.Value;
                foreach (var k in attacks)
                {
                    if (k.AttackStatus == AutoAttack.AttackState.Completed || !k.IsValid() || k.Eta < -200 || !k.Target.IsValid || k.Target.NetworkId != target.NetworkId || Game.TickCount - k.DetectTime > 3000)
                    {
                        continue;
                    }

                    if (k.Eta < time)
                    {
                        predictedDamage += (float)k.Damage;
                    }
                }
            }

            var health = target.Health - predictedDamage;
            return health;
        }

        public float GetLaneClearHealthPrediction(Obj_AI_Base target, int time)
        {
            float predictedDamage = 0;
            var rTime = time;

            foreach (var m in Attacks.OutBoundAttacks)
            {
                var attacks = m.Value;
                foreach (var k in attacks)
                {
                    if (k.NetworkId != target.NetworkId || Game.TickCount - k.DetectTime > rTime)
                    {
                        continue;
                    }

                    predictedDamage += (float)k.Damage;
                }
            }

            return target.Health - predictedDamage;
        }


        private int LastCleanUp { get; set; }

        private void Game_OnUpdate()
        {
            //Limit the clean up to every 2 seconds
            if (Game.TickCount - this.LastCleanUp <= 1000)
            {
                return;
            }

            //Remove attacks more than 5 seconds old 
            foreach (var kvp in Attacks.OutBoundAttacks)
            {
                foreach (var attack in kvp.Value)
                {
                    if (!attack.Sender.IsValid || attack.Sender.IsDead || !attack.Target.IsValid || attack.Target.IsDead)
                    {
                        attack.AttackStatus = AutoAttack.AttackState.Completed;
                    }
                }

                kvp.Value.RemoveAll(x => x.CanRemoveAttack);
            }

            this.LastCleanUp = Game.TickCount;
        }

        private static void Obj_AI_Base_OnProcessAutoAttack(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs e)
        {
            //Ignore auto attacks happening too far away
            if (sender == null || !sender.IsValidTarget(4000, true) || sender is Obj_AI_Hero)
            {
                return;
            }

            //Only process for minion targets
            var targetM = e.Target as Obj_AI_Minion;
            if (targetM == null)
            {
                return;
            }


            AutoAttack attack;

            if (sender.IsMelee)
            {
                attack = new MeleeAttack(sender, targetM, 0);
            }

            else
            {
                attack = new RangedAttack(sender, targetM, 0);
            }

            Attacks.AddAttack(attack);
        }

        private static void GameObject_OnCreate(GameObject gsender)
        {
            if (gsender == null || !gsender.IsValid)
            {
                return;
            }

            var mc = gsender as MissileClient;
            if (mc == null || !mc.IsValid || mc.Target == null || !mc.Target.IsValid)
            {
                return;
            }

            var sender = mc.SpellCaster;
            var target = mc.Target as Obj_AI_Minion;
            if (sender == null || !sender.IsValid || sender.IsMelee || sender is Obj_AI_Hero)
            {
                return;
            }

            if (target == null || !target.IsValid)
            {
                return;
            }

            //Ignore the missile if it is not an auto attack...
            if (!IsAutoAttack(mc.SpellData.Name))
            {
                return;
            }

            var attacks = Attacks.GetOutBoundAttacks(mc.SpellCaster.NetworkId);
            var attack = attacks?.MaxBy(x => x.DetectTime);
            if (attack != null)
            {
                if (attack is RangedAttack rangedAttack)
                {
                    rangedAttack.MissileCreated(mc);
                }
            }
        }

        private static void GameObject_OnDestroy(GameObject sender)
        {
            if (sender == null || !sender.IsValid)
            {
                return;
            }

            var mc = sender as MissileClient;
            if (mc == null || mc.SpellCaster == null || mc.SpellCaster.IsMelee || mc.SpellCaster is Obj_AI_Hero)
            {
                return;
            }

            var target = mc.Target as Obj_AI_Base;
            if (target == null)
            {
                return;
            }

            var attacks = Attacks.GetOutBoundAttacks(mc.SpellCaster.NetworkId);

            //Might be destroying too eearly, attack may not land when missile destroyed
            //add the missile to the attack object
            var attack = attacks?.Where(x => x.Target.NetworkId == mc.Target.NetworkId && x.AttackStatus != AutoAttack.AttackState.Completed).MinBy(x => x.DetectTime);
            if (attack != null)
            {
                if (attack is RangedAttack rangedAttack)
                {
                    rangedAttack.MissileDestroyed();
                }
            }
        }

        public abstract class AutoAttack
        {
            protected AutoAttack(Obj_AI_Base sender, Obj_AI_Base target)
            {
                this.AttackStatus = AttackState.Detected;
                this.DetectTime = Game.TickCount - Game.Ping / 2;
                this.Sender = sender;
                this.Target = target;
                this.SNetworkId = sender.NetworkId;
                this.NetworkId = target.NetworkId;

                this.Damage = sender.GetAutoAttackDamage(target);

                this.StartPosition = sender.ServerPosition;

                this.AnimationDelay = (int)(sender.AttackCastDelay * 1000);
            }

            public bool CanRemoveAttack => Game.TickCount - this.DetectTime > 5000;

            public double Damage { get; set; }

            public int SNetworkId { get; set; }
            public int NetworkId { get; set; }

            public Obj_AI_Base Sender { get; set; }
            public Obj_AI_Base Target { get; set; }

            public int AnimationDelay { get; set; }

            public Vector3 StartPosition { get; set; }

            public float Distance => this.StartPosition.Distance(this.Target.Position) - this.Sender.BoundingRadius - this.Target.BoundingRadius;

            public int DetectTime { get; set; }

            public abstract int EstArrivalTime { get; }

            public abstract int Eta { get; }

            public int ExtraDelay { get; set; }

            public AttackState AttackStatus { get; set; }

            public enum AttackState
            {
                None,
                Detected,
                MissileCreated,
                MissileDestroyed,
                Completed
            }

            public void Completed()
            {
                this.AttackStatus = AttackState.Completed;
            }

            public bool IsValid()
            {
                return this.Sender.IsValid && this.Target.IsValid;
            }

            public virtual bool Active { get; set; }
        }


        public class RangedAttack : AutoAttack
        {
            public RangedAttack(Obj_AI_Base sender, Obj_AI_Base target, int extraDelay) : base(sender, target)
            {
                this.ExtraDelay = extraDelay + 60;
            }

            public MissileClient Missile { get; set; }

            public override bool Active => this.Eta > 0;

            public override int Eta => (int)this.RegularEta;

            public int TravelTime()
            {
                if (this.Target.IsValid)
                {
                    return (int)(this.StartPosition.Distance(this.Target.ServerPosition) / this.Sender.BasicAttack.MissileSpeed * 1000);
                }

                return 0;
            }

            public int TotalTimeToReach => this.AnimationDelay + this.TravelTime() + this.ExtraDelay;

            public override int EstArrivalTime => this.DetectTime + this.TotalTimeToReach;

            public int ElapsedTime => Game.TickCount - this.DetectTime;

            public float RegularEta => this.EstArrivalTime - Game.TickCount;

            public int MissileCreationTime { get; set; }

            public int MissileDestructionTime { get; set; }

            public void MissileCreated(MissileClient mc)
            {
                if (mc.IsValid)
                {
                    this.MissileCreationTime = Game.TickCount;
                    this.AttackStatus = AttackState.MissileCreated;
                    this.Missile = mc;
                }
            }

            public void MissileDestroyed()
            {
                this.MissileDestructionTime = Game.TickCount;
                this.AttackStatus = AttackState.Completed;
            }
        }

        public class MeleeAttack : AutoAttack
        {
            public MeleeAttack(Obj_AI_Base sender, Obj_AI_Base target, int extraDelay) : base(sender, target)
            {
                this.ExtraDelay = extraDelay + 60;
            }

            public override bool Active => this.Eta > 0;

            public override int EstArrivalTime => this.DetectTime + (int)(this.Sender.AttackCastDelay * 1000);

            public override int Eta => Math.Max(0, this.EstArrivalTime - Game.TickCount) + this.ExtraDelay;
        }

        private class Attacks
        {
            public static Dictionary<int, List<AutoAttack>> OutBoundAttacks { get; } = new Dictionary<int, List<AutoAttack>>();

            public static void AddAttack(AutoAttack attack)
            {
                AddOutBoundAttack(attack);
            }

            private static void AddOutBoundAttack(AutoAttack attack)
            {
                var k = attack.Sender.NetworkId;
                if (!OutBoundAttacks.ContainsKey(k))
                {
                    OutBoundAttacks[k] = new List<AutoAttack>();
                }

                if (attack is MeleeAttack)
                {
                    foreach (var a in OutBoundAttacks[k])
                    {
                        a.Completed();
                    }
                }

                OutBoundAttacks[k].Add(attack);
            }

            public static void RemoveOutBoundAttack(AutoAttack attack)
            {
                var k = attack.Sender.NetworkId;
                OutBoundAttacks[k].Remove(attack);
            }

            public static List<AutoAttack> GetOutBoundAttacks(int unitId)
            {
                OutBoundAttacks.TryGetValue(unitId, out List <AutoAttack> attacks);
                return attacks;
            }
        }
    }
}
