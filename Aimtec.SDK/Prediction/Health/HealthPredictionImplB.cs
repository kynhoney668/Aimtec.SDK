namespace Aimtec.SDK.Prediction.Health
{
    using System.Collections.Generic;
    using System.Linq;

    using Aimtec.SDK.Damage;
    using Aimtec.SDK.Extensions;
    using Aimtec.SDK.Menu;
    using Aimtec.SDK.Menu.Components;
    using Aimtec.SDK.Menu.Config;

    internal class HealthPredictionImplB : IHealthPrediction
    {
        internal Menu Config { get; set; }

        public HealthPredictionImplB()
        {
            Obj_AI_Base.OnProcessAutoAttack += this.Obj_AI_Base_OnProcessAutoAttack;
            GameObject.OnCreate += this.GameObject_OnCreate;
            GameObject.OnDestroy += this.GameObject_OnDestroy;
            Game.OnUpdate += this.Game_OnUpdate;
            Obj_AI_Base.OnPerformCast += this.Obj_AI_Base_OnPerformCast;
            SpellBook.OnStopCast += this.SpellBook_OnStopCast;

            this.Config = new Menu("HealthPred", "HealthPrediction")
                              {
                                  new MenuSlider("ExtraDelay", "Extra Delay", 0, 0, 150),
                              };

            AimtecMenu.Instance.Add(this.Config);
        }

        private void GameObject_OnCreate(GameObject gsender)
        {
            var mc = gsender as MissileClient;
            if (mc == null || !mc.IsValid)
            {
                return;
            }

            var sender = mc.SpellCaster;
            var target = mc.Target;

            var targetBase = target as Obj_AI_Base;

            if (sender == null || targetBase == null || !sender.IsValid || !target.IsValid || target.IsDead
                || sender.IsDead || sender.IsMe || sender.Distance(ObjectManager.GetLocalPlayer()) > 4000)
            {
                return;  
            }


            var attack = new RangedAttack(sender, targetBase, mc, 0);
            Attacks.AddAttack(attack);
        }

        private void Obj_AI_Base_OnProcessAutoAttack(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs e)
        {
            //Ignore auto attacks happening too far away
            if (sender == null || !sender.IsValidTarget(4000, true) || !sender.IsMelee || sender is Obj_AI_Hero)
            {
                return;
            }

            //Only process for minion targets
            var targetM = e.Target as Obj_AI_Minion;
            if (targetM == null)
            {
                return;
            }

            if (sender.IsMelee)
            {
                AutoAttack attack = new MeleeAttack(sender, targetM, 0);
                Attacks.AddAttack(attack);
            }
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
                    ob.Clear();
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


        public float GetPrediction(Obj_AI_Base target, int time)
        {
            float predictedDamage = 0;
            foreach (var m in Attacks.OutBoundAttacks)
            {
                var attacks = m.Value;

                foreach (var k in attacks)
                {
                    if (k.HasReached() || !k.IsValid())
                    {
                        continue;
                    }

                    if (k.Target.NetworkId != target.NetworkId || Game.TickCount - k.DetectTime > 3000)
                    {
                        continue;
                    }

                    var mlt = Game.TickCount + time;
                    var alt = Game.TickCount + k.ETA;

                    if (mlt - alt > 100 + this.Config["ExtraDelay"].Value)
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


        private void GameObject_OnDestroy(GameObject sender)
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

            var attack = attacks?.Where(x => x.Target.NetworkId == mc.Target.NetworkId && x.AttackStatus != AutoAttack.AttackState.Completed && x is RangedAttack).MinBy(x => x.DetectTime);
            if (attack != null)
            {
                var rattack = attack as RangedAttack;
                if (rattack != null) rattack.AttackStatus = AutoAttack.AttackState.Completed;
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

            public float Distance => this.StartPosition.Distance(this.Target.Position) - this.Sender.BoundingRadius
                                     - this.Target.BoundingRadius;

            public int DetectTime { get; set; }

            public abstract int LandTime { get; }

            public virtual int ETA => this.LandTime - Game.TickCount;

            public int ExtraDelay { get; set; }

            public AttackState AttackStatus { get; set; }

            public enum AttackState
            {
                None,

                Detected,

                Completed
            }

            public virtual bool IsValid()
            {
                return this.Sender.IsValid && this.Target.IsValid;
            }

            public int ElapsedTime => Game.TickCount - this.DetectTime;

            public abstract bool HasReached();
        }

        public class RangedAttack : AutoAttack
        {
            public RangedAttack(Obj_AI_Base sender, Obj_AI_Base target, MissileClient mc, int extraDelay) : base(sender, target)
            {
                this.ExtraDelay = extraDelay;
                this.Missile = mc;
                this.StartPosition = mc.Position;
            }

            public override bool HasReached()
            {
                if (this.Missile == null || !this.IsValid() || this.AttackStatus == AttackState.Completed || this.ETA < -200)
                {
                    return true;
                }

                return false;
            }

            public override bool IsValid()
            {
                return this.Missile.IsValid && this.Sender.IsValid && this.Target.IsValid;
            }

            public MissileClient Missile { get; set; }

            public Vector3 EstimatedPosition
            {
                get
                {
                    var dist = (this.Missile.SpellData.MissileSpeed / 1000) * (Game.TickCount - this.DetectTime);
                    return this.StartPosition.Extend(this.Missile.EndPosition, dist);
                }
            }
             
            //isvalidcheckbefore
            public int TimeToLand
            {
                get
                {
                    return (int)(this.Missile.ServerPosition.Distance(this.Missile.EndPosition)
                                 / this.Missile.SpellData.MissileSpeed * 1000);
                }
            }

            public override int LandTime => Game.TickCount + this.TimeToLand;
        }

        public class MeleeAttack : AutoAttack
        {
            public MeleeAttack(Obj_AI_Base sender, Obj_AI_Base target, int extraDelay) : base(sender, target)
            {
                this.ExtraDelay = extraDelay + 100;
            }

            public override bool HasReached()
            {
                if (this.AttackStatus == AttackState.Completed || !this.IsValid() || this.ETA < -100)
                {
                    return true;
                }

                return false;
            }

            public override int LandTime => this.DetectTime + (int)(this.Sender.AttackCastDelay * 1000) + this.ExtraDelay;

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
                        a.AttackStatus = AutoAttack.AttackState.Completed;
                        
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
