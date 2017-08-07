using Aimtec.SDK.Damage;
using Aimtec.SDK.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aimtec.SDK.Prediction.Health
{
    public class TurretAttackManager
    {
        public static Dictionary<int, TurretData> Turrets = new Dictionary<int, TurretData>();

        public static List<TurretData> TurretsList => Turrets.Values.ToList();

        static TurretAttackManager()
        {
            if (Game.Mode == GameMode.Running)
            {
                Start();
                return;
            }

            Game.OnStart += Start;
        }

        static void Start()
        {
            //Generate turret data for each turret in game
            foreach (var turret in ObjectManager.Get<Obj_AI_Turret>())
            {
                Turrets[turret.NetworkId] = new TurretData(turret);
            }

            Obj_AI_Base.OnProcessAutoAttack += Obj_AI_Base_OnProcessAutoAttack;
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDestroy += GameObject_OnDestroy;
            Game.OnUpdate += Game_OnUpdate;
        }


        public static TurretData GetTurretData(int netID)
        {
            Turrets.TryGetValue(netID, out TurretData td);
            return td;
        }

        /*
        public static AttackableUnit GetUnderTurretAutoTarget()
        {
            var units = ObjectManager.Get<Obj_AI_Base>().Where(x => x.IsValidAutoRange());

            foreach (var unit in units)
            {
                var turretd = GetNearestTurretData(unit, TurretTeam.Enemy);

                var castDelay = ObjectManager.GetLocalPlayer().AttackCastDelay * 1000;
                var travelDelay = (ObjectManager.GetLocalPlayer().Distance(unit) / ObjectManager.GetLocalPlayer().BasicAttack.MissileSpeed) * 1000;
                var pingDelay = Game.Ping / 2;
                var playerDamage = ObjectManager.GetLocalPlayer().GetAutoAttackDamage(unit);

                var totalDelay = castDelay + travelDelay + pingDelay;

                //In turret range
                if (turretd != null)
                {
                    var turretDamage = turretd.Turret.GetAutoAttackDamage(unit);

                    if (turretd.LastAttack.AttackStatus == TurretData.TurretAttack.AttackState.Casted)
                    {
                        var eta = turretd.LastAttack.PredictedMissileETA;
                        //Turret attack reaches this target sooner
                        if (eta < totalDelay)
                        {//Attack a killable minion while this happens if possible
                            return GetKillableTarget();
                        }

                        //My attack reaches sooner
                        else if (eta > totalDelay)
                        {
                            
                        }
                    }

                    else if (turretd.LastAttack.AttackStatus == TurretData.TurretAttack.AttackState.CreatedMissile)
                    {

                    }

                }
            }
        }
        */
        

        public static AttackableUnit GetKillableTarget()
        {
            return null;
        }


        public enum TurretTeam
        {
            Ally,
            Enemy
        }

        public static TurretData GetNearestTurretData(Obj_AI_Base unit, TurretTeam team)
        {
            var t = TurretsList.Where(x => (team == TurretTeam.Ally ? x.Turret.IsValid && x.Turret.Team == unit.Team : x.Turret.Team != unit.Team)).MinBy(x => x.Turret.Distance(unit));

            if (t != null)
            {
                return t;
            }

            return null;
        }

        public static bool UnderTurret(Obj_AI_Base unit, Obj_AI_Base turret)
        {
            return unit.Distance(turret) < 950;
        }

        public static int GetTurretDamage(Obj_AI_Base unit, int time)
        {
            var totalDamage = 0;

            var t = GetNearestTurretData(unit, TurretTeam.Enemy);

            if (t != null && UnderTurret(unit, t.Turret))
            {
                foreach (var attack in t.Attacks)
                {
                    if (attack.AttackStatus == TurretData.TurretAttack.AttackState.Completed)
                    {
                        continue;
                    }

                    if (attack.PredictedMissileETA < time)
                    {
                        var damage = t.Turret.GetAutoAttackDamage(unit);
                        totalDamage += totalDamage;
                    }
                }
            }

            return totalDamage;
        }

        private static void Game_OnUpdate()
        {
            foreach (var turret in Turrets.Values)
            {
                turret.OnUpdate();
            }
        }


        private static void Obj_AI_Base_OnProcessAutoAttack(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs e)
        {
            if (sender == null || e.Target == null)
            {
                return;
            }

            var bTarget = e.Target as Obj_AI_Base;

            if (bTarget == null)
            {
                return;
            }


            if (sender is Obj_AI_Turret tSender)
            {
                Turrets[tSender.NetworkId].OnAttack(tSender, bTarget, e);
            }
        }

        private static void GameObject_OnCreate(GameObject sender)
        {
            if (sender == null)
            {
                return;
            }

            var attack = sender as MissileClient;

            if (attack == null)
            {
                return;
            }


            if (attack.SpellCaster is Obj_AI_Turret tSender)
            {
                Turrets[tSender.NetworkId].OnMissileCreated(attack);
            }

        }


        private static void GameObject_OnDestroy(GameObject sender)
        {
            if (sender == null)
            {
                return;
            }

            var attack = sender as MissileClient;

            if (attack == null)
            {
                return;
            }


            if (attack.SpellCaster is Obj_AI_Turret tSender)
            {
                Turrets[tSender.NetworkId].OnMissileDestroyed(attack);
            }
        }

        public class TurretData
        {
            public Obj_AI_Turret Turret { get; set; }

            public bool TurretActive => Game.TickCount - this.LastFireTime < 2500;

            public TurretAttack LastAttack { get; set; }

            public int LastFireTime { get; set; }

            public Obj_AI_Base LastTarget { get; set; }

            public List<TurretAttack> Attacks = new List<TurretAttack>();

            public TurretData(Obj_AI_Turret turret)
            {
                this.Turret = turret;
            }

            public void OnAttack(Obj_AI_Turret sender, Obj_AI_Base target, Obj_AI_BaseMissileClientDataEventArgs e)
            {
                this.LastFireTime = Game.TickCount;
                this.LastTarget = target;

                var tAttack = new TurretAttack
                {
                    StartTime = Game.TickCount,
                    Sender = sender,
                    Target = target,
                    AttackStatus = TurretAttack.AttackState.Casted,
                };

                this.LastAttack = tAttack;

                Attacks.Add(tAttack);
            }

            //Turret Attack Created
            public void OnMissileCreated(MissileClient mc)
            {
                var attack = this.Attacks.Where(x => x.AttackStatus == TurretAttack.AttackState.Casted).MaxBy(x => x.StartTime);
                if (attack != null)
                {
                    attack.MissileCreationTime = Game.TickCount;
                    attack.Missile = mc;
                    attack.AttackStatus = TurretAttack.AttackState.CreatedMissile;
                }
            }

            //Turret Attack Destroyed/Landed
            public void OnMissileDestroyed(MissileClient mc)
            {
                var attack = this.Attacks.Find(x => x.Missile != null && x.Missile.NetworkId == mc.NetworkId && x.AttackStatus == TurretAttack.AttackState.CreatedMissile);

                if (attack != null)
                {
                    attack.Inactive = true;
                    attack.RealEndTime = Game.TickCount;
                    attack.AttackStatus = TurretAttack.AttackState.Completed;
                }
            }

            public void OnUpdate()
            {
                foreach (var item in Attacks.ToList())
                {
                    if (Game.TickCount - item.StartTime > 3000)
                    {
                        Attacks.Remove(item);
                    }
                }
            }

            public class TurretAttack
            {
                public int MissileCreationTime { get; set; }
                public MissileClient Missile { get; set; }


                public int StartTime { get; set; }
                public Obj_AI_Turret Sender { get; set; }
                public Obj_AI_Base Target { get; set; }
                public float DistanceWOBR => (int)(Target.Distance(Sender) - Sender.BoundingRadius - Target.BoundingRadius);

                public int PredictedLandTime
                {
                    get { return this.StartTime + (int)(Sender.AttackCastDelay * 1000) + (int)(DistanceWOBR / Sender.BasicAttack.MissileSpeed) * 1000 - Game.Ping / 2; }
                }

                public int PredictedETA
                {
                    get { return PredictedLandTime - Game.TickCount; }
                }

                public int PredictedMissileETA
                {
                    get
                    {
                        if (this.Missile != null && this.Missile.IsValid)
                        {
                            var position = Missile.ServerPosition;
                            var distance = Missile.Distance(Target);

                            var travelTime = (distance / this.Sender.BasicAttack.MissileSpeed) * 1000 - Game.Ping / 2;

                            return (int)travelTime;
                        }

                        return this.PredictedETA;
                    }
                }

                public AttackState AttackStatus { get; set; }

                //Attack is destroyed
                public bool Inactive { get; set; } = false;

                public int RealEndTime { get; set; }

                public enum AttackState
                {
                    Casted,
                    CreatedMissile,
                    Completed
                }
            }
        }
    }
}
