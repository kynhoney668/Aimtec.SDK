namespace Aimtec.SDK.Prediction.Health
{
    using System;
    using System.Collections.Generic;

    using Damage;
    using Aimtec.SDK.Extensions;


    class HealthPredictionImplB : IHealthPrediction
    {
        private Obj_AI_Hero Player => ObjectManager.GetLocalPlayer();

        private readonly Dictionary<int, List<Attack>> incomingAttacks = new Dictionary<int, List<Attack>>();

        private readonly Dictionary<int, TurretAttackData> turretData = new Dictionary<int, TurretAttackData>();

        internal HealthPredictionImplB()
        {
            Game.OnUpdate += this.GameOnOnUpdate;
            Obj_AI_Base.OnProcessAutoAttack += this.ObjAiBaseOnOnProcessAutoAttack;
            SpellBook.OnStopCast += this.SpellBookOnOnStopCast;
        }


        private void GameOnOnUpdate()
        {
            // remove attacks where either the target or sender has become invalid or when they are no longer active
            foreach (var value in this.incomingAttacks.Values)
            {
                value.RemoveAll(x => !x.Target.IsValidTarget(5000, true) || !x.Sender.IsValidTarget(5000, true) || x.Active || Game.TickCount - x.CastTime > 3500);
            }
        }

        private void SpellBookOnOnStopCast(Obj_AI_Base sender, SpellBookStopCastEventArgs e)
        {
            if (!e.DestroyMissile || !e.StopAnimation || (!(sender is Obj_AI_Minion) || sender.Type == GameObjectType.obj_AI_Turret))
            {
                return;
            }

            foreach (var value in this.incomingAttacks.Values)
            {
                value.RemoveAll(x => (x.Sender.NetworkId == sender.NetworkId));
            }
        }


        private void ObjAiBaseOnOnProcessAutoAttack(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs args)
        {
            //Only detect attacks from and to minions close to us
            if (sender.Distance(this.Player) > 3500 || sender is Obj_AI_Hero)
            {
                return;
            }


            foreach (var value in this.incomingAttacks.Values)
            {
                value.RemoveAll(x => (x.Sender.NetworkId == sender.NetworkId));
            }


            var target = args.Target as Obj_AI_Base;

            if (target != null)
            {

                if (sender.IsTurret)
                {
                    if (!this.turretData.ContainsKey(sender.NetworkId))
                    {
                        this.turretData.Add(sender.NetworkId, new TurretAttackData());
                    }

                    var data = this.turretData[sender.NetworkId];

                    if (data.LastTarget != null)
                    {
                        if (data.LastTarget.NetworkId == target.NetworkId && data.LastAttackTime < 2000)
                        {
                            data.shotNumber++;
                        }
                    }

                    else
                    {
                        data.shotNumber = 0;
                    }


                    data.LastTarget = target;
                    data.LastAttackTime = Game.TickCount;

                    TurretAttack auto = new TurretAttack(sender, target, args, data.shotNumber);

                    if (!this.incomingAttacks.ContainsKey(auto.Target.NetworkId))
                    {
                        this.incomingAttacks.Add(auto.Target.NetworkId, new List<Attack>());
                    }

                    this.incomingAttacks[auto.Target.NetworkId].Add(auto);
                }

                else
                {

                    AutoAttack auto = new AutoAttack(sender, target, args);

                    if (!this.incomingAttacks.ContainsKey(auto.Target.NetworkId))
                    {
                        this.incomingAttacks.Add(auto.Target.NetworkId, new List<Attack>());
                    }

                    this.incomingAttacks[auto.Target.NetworkId].Add(auto);

                }
            }
        }

        public float GetPredictedDamage(Obj_AI_Base unit, int time)
        {
            //If there is no incoming auto attacks detected for this unit, then it is not taking damage, so return 0.
            if (!this.incomingAttacks.ContainsKey(unit.NetworkId))
            {
                return 0;
            }

            var incAttacksUnit = this.incomingAttacks[unit.NetworkId];

            float predictedDmg = 0f;

            foreach (var attack in incAttacksUnit)
            {
                //if this attack will take longer than the specified time to reach the target, then ignore it
                if (attack.ETA > time)
                {
                    continue;
                }

                float dmg = 0;

                if (attack is AutoAttack)
                {
                    dmg = (float) attack.Sender.GetAutoAttackDamage(unit); //calc dmg from source to this unit
                }

                //Must be turret attack
                else
                {
                    var tAttack = attack as TurretAttack;
                    if (tAttack != null)
                    {
                        dmg = (1 + tAttack.DmgPercentageIncrease) * (float) attack.Sender.GetAutoAttackDamage(unit);
                    }
                }

                predictedDmg += dmg;
            }
            return predictedDmg;
        }

        public float GetPrediction(Obj_AI_Base unit, int time)
        {
            var pred = Math.Max(0, unit.Health - this.GetPredictedDamage(unit, time));
            return pred;
        }

        public class Attack
        {
            public Attack(Obj_AI_Base sender, Obj_AI_Base target, Obj_AI_BaseMissileClientDataEventArgs args)
            {
                this.Sender = sender;
                this.Target = target;
                this.StartPosition = sender.Position;
                this.Missilespeed = args.SpellData.MissileSpeed;
                this.args = args;
                this.CastTime = Game.TickCount - Game.Ping / 2;
            }

            public Obj_AI_Base Sender { get; set; }
            public Obj_AI_Base Target { get; set; }
            public Vector3 StartPosition { get; set; }
            public float Missilespeed { get; set; }
            public float CastTime { get; set; }
            public Obj_AI_BaseMissileClientDataEventArgs args;

            public float Distance => this.StartPosition.Distance(this.Target.Position) - this.Sender.BoundingRadius - this.Target.BoundingRadius;

            public virtual float TravelTime { get; set; }

            //Gets the time passed by since this auto attack was detected
            public float TimeElapsed => Game.TickCount - this.CastTime;

            //Gets the time left until this auto reaches target by subtracting the time elapsed from total travel time
            public float ETA => (this.TravelTime - this.TimeElapsed);

            //If there is time left until arrival, this auto has not arrived yet and is active, otherwise this auto attack has already reached the target and is inactive
            public virtual bool Active => this.ETA >= 0;
        }

        public class AutoAttack : Attack
        {
            public AutoAttack(Obj_AI_Base sender, Obj_AI_Base target, Obj_AI_BaseMissileClientDataEventArgs args) : base(sender, target, args)
            {
                this.Melee = sender.IsMelee;
            }

            public override float TravelTime => (this.Melee ? 0 : (this.Distance / this.Missilespeed) * 1000) + (this.Sender.AttackCastDelay * 1000) + 100;

            public override bool Active => this.ETA >= 0 && this.Melee;

            public bool Melee { get; set; }
        }

        public class TurretAttack : Attack
        {
            public TurretAttack(Obj_AI_Base sender, Obj_AI_Base target, Obj_AI_BaseMissileClientDataEventArgs args, int shotNumber) : base(sender, target, args)
            {
                this.ShotNumber = shotNumber;
            }

            public override float TravelTime => ((this.Distance / this.Missilespeed) * 1000) + (this.Sender.AttackCastDelay * 1000);
            public int ShotNumber { get; set; }
            public int HeatAmount => Math.Min(this.ShotNumber * 6, 120);
            public float DmgPercentageIncrease => Math.Min(125f, this.HeatAmount * 1.05f) / 100;
        }

        class TurretAttackData
        {
            public Obj_AI_Base LastTarget;
            public int shotNumber = 0;
            public float LastAttackTime;
        }
    }
}
