using System;

namespace Aimtec.SDK.Orbwalking
{
    using System.Collections.Generic;
    using System.Linq;

    using Aimtec.SDK.Damage;
    using Aimtec.SDK.Extensions;
    using Aimtec.SDK.Menu;
    using Aimtec.SDK.Menu.Components;
    using Aimtec.SDK.Menu.Config;
    using Aimtec.SDK.Prediction.Health;
    using Aimtec.SDK.TargetSelector;
    using Aimtec.SDK.Util;
    using NLog;
  
    internal class OrbwalkingImpl : AOrbwalker
    {
        internal OrbwalkingImpl()
        {
            this.Initialize();
        }

        public override void Attach(IMenu menu)
        {
            if (!this.Attached)
            {
                this.Attached = true;
                menu.Add(this.Config);
                Obj_AI_Base.OnProcessAutoAttack += this.ObjAiHeroOnProcessAutoAttack;
                Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
                Game.OnUpdate += this.Game_OnUpdate;
                SpellBook.OnStopCast += SpellBook_OnStopCast;
                RenderManager.OnRender += RenderManager_OnRender;
            }
            else
            {
                Logger.Info("This Orbwalker instance is already attached to a Menu.");
            }
        }

        public override float WindUpTime => this.AnimationTime + this.ExtraWindUp;

        // TODO this completely breaks the modular design, Orbwalker and health prediction shouldnt be tightly coupled!
        private HealthPredictionImplB HealthPrediction { get; } = new HealthPredictionImplB();

        private AttackableUnit LastTarget { get; set; }

        public float AnimationTime => Player.AttackCastDelay * 1000;

        public float AttackCoolDownTime => (Player.AttackDelay * 1000) - this.AttackDelayReduction;

        protected bool AttackReady => (Game.TickCount + Game.Ping / 2) - this.ServerAttackDetectionTick
                                      >= this.AttackCoolDownTime;

        /// <summary>
        ///     Gets or sets the Forced Target 
        /// </summary>
        private AttackableUnit ForcedTarget { get; set; }

        /// <summary>
        ///     The time the last attack command was sent (determined locally)
        /// </summary>
        protected float LastAttackCommandSentTime;

        public override bool IsWindingUp
        {
            get
            {
                var detectionTime = Math.Max(this.ServerAttackDetectionTick, this.LastAttackCommandSentTime);
                return (Game.TickCount + Game.Ping / 2) - detectionTime <= this.WindUpTime;
            }
        }

        //Menu Getters
        private int HoldPositionRadius => this.Config["Misc"]["holdPositionRadius"].Value;

        private int ExtraWindUp => this.Config["Misc"]["extraWindup"].Value;

        private int AttackDelayReduction => this.Config["Advanced"]["attackDelayReduction"].Value;

        private bool DrawHoldPosition => this.Config["Drawings"]["drawHoldRadius"].Enabled;

        private bool DrawAttackRange => this.Config["Drawings"]["drawAttackRange"].Enabled;


        //Members
        private float ServerAttackDetectionTick { get; set; }
       
        private bool Attached { get; set; }

        private void Initialize()
        {
            this.Config = new Menu("Orbwalker", "Orbwalker")
            {
                new Menu("Advanced", "Advanced")
                {
                    new MenuSlider("attackDelayReduction", "Attack Delay Reduction", 90, 0, 180, true),
                },

                new Menu("Drawings", "Drawings")
                {
                    new MenuBool("drawAttackRange", "Draw Attack Range", true),
                    new MenuBool("drawHoldRadius", "Draw Hold Radius", false),
                },

                new Menu("Misc", "Misc")
                {
                    new MenuSlider("holdPositionRadius", "Hold Radius", 50, 0, 400, true),
                    new MenuSlider("extraWindup", "Additional Windup", 30, 0, 200, true),
                    new MenuBool("noBlindAA", "No AA when Blind", true, true),
                }
            };

            this.AddMode(Combo = new OrbwalkerMode("Combo", GlobalKeys.ComboKey, GetHeroTarget, null));
            this.AddMode(LaneClear = new OrbwalkerMode("Laneclear", GlobalKeys.WaveClearKey, GetLaneClearTarget, null));
            this.AddMode(LastHit = new OrbwalkerMode("Lasthit", GlobalKeys.LastHitKey, GetLastHitTarget, null));
            this.AddMode(Mixed = new OrbwalkerMode("Mixed", GlobalKeys.MixedKey, GetMixedModeTarget, null));
        }


        protected void Game_OnUpdate()
        {
            Orbwalk();
        }

        public bool BlindCheck()
        {
            if (!this.Config["Misc"]["noBlindAA"].Enabled)
            {
                return true;
            }

            if (!Player.ChampionName.Equals("Kalista") && !Player.ChampionName.Equals("Twitch"))
            {
                if (Player.BuffManager.HasBuffOfType(BuffType.Blind))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool CanMove()
        {
            return this.CanMove(this.GetActiveMode());
        }

        public override bool CanAttack()
        {
            return this.CanAttack(this.GetActiveMode());
        }

        public bool CanAttack(OrbwalkerMode mode)
        {
            if (mode == null)
            {
                return false;
            }

            if (!this.AttackingEnabled || !mode.AttackingEnabled)
            {
                return false;
            }

            if (!this.BlindCheck())
            {
                return false;
            }

            if (Player.ChampionName.Equals("Jhin") && Player.HasBuff("JhinPassiveReload"))
            {
                return false;
            }

            if (this.NoCancelChamps.Contains(Player.ChampionName))
            {
                return true;
            }

            if (this.IsWindingUp)
            {
                return false;
            }
            
            return this.AttackReady;
        }

        public bool CanMove(OrbwalkerMode mode)
        {
            if (mode == null)
            {
                return false;
            }

            if (!this.MovingEnabled || !mode.MovingEnabled)
            {
                return false;
            }

            if (Player.Distance(Game.CursorPos) < this.HoldPositionRadius)
            {
                return false;
            }

            if (NoCancelChamps.Contains(Player.ChampionName))
            {
                return true;
            }

            if (this.IsWindingUp)
            {
                return false;
            }

            return true;
        }

        private void RenderManager_OnRender()
        {
            if (this.DrawAttackRange)
            {
                RenderManager.RenderCircle(Player.Position, Player.AttackRange + Player.BoundingRadius, 30, System.Drawing.Color.DeepSkyBlue);
            }

            if (this.DrawHoldPosition)
            {
                RenderManager.RenderCircle(Player.Position, this.HoldPositionRadius, 30, System.Drawing.Color.White);
            }
        }

        private void SpellBook_OnStopCast(Obj_AI_Base sender, SpellBookStopCastEventArgs e)
        {
            if (sender.IsMe && (e.DestroyMissile || e.ForceStop || e.StopAnimation))
            {
                this.ResetAutoAttackTimer();
            }
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs e)
        {
            if (sender.IsMe)
            {
                /* Detect Caitlyn W Headshot */
                if (Player.ChampionName == "Caitlyn")
                {
                    if (e.SpellData.Name.ToLower().Contains("caitlynheadshotmissile"))
                    {
                        this.ServerAttackDetectionTick = Game.TickCount;
                    }
                }
       
                if (this.IsReset(e.SpellData.Name))
                {
                    this.ResetAutoAttackTimer();
                }
            }
        }

        protected void ObjAiHeroOnProcessAutoAttack(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs args)
        {
            if (sender.IsMe)
            {
                var targ = args.Target as AttackableUnit;
                if (targ != null)
                {
                    this.ServerAttackDetectionTick = Game.TickCount - Game.Ping / 2;
                    this.LastTarget = targ;
                    this.ForcedTarget = null;
                    DelayAction.Queue((int)WindUpTime, () => FirePostAttack(targ));
                }
            }
        }

        public override void Orbwalk()
        {
            OrbwalkerMode mode = this.GetActiveMode();

            if (mode == null)
            {
                return;
            }

            /// <summary>
            ///     Execute the specific logic for this mode if any
            /// </summary>
            mode.Execute();

            if (!mode.BaseOrbwalkingEnabled)
            {
                return;
            }

            if (this.CanAttack(mode))
            {
                var target = GetTarget(mode);
                if (target != null)
                {
                }
            }

            if (this.CanMove(mode))
            {
                this.Move(Game.CursorPos);
            }
        }

        public override void ResetAutoAttackTimer()
        {
            this.ServerAttackDetectionTick = 0;
            this.LastAttackCommandSentTime = 0;
        }

        public override void ForceTarget(AttackableUnit unit)
        {
            this.ForcedTarget = unit;
        }

        AttackableUnit GetHeroTarget()
        {
            return TargetSelector.Implementation.GetOrbwalkingTarget();
        }

        AttackableUnit GetLastHitTarget()
        {
            return this.GetLastHitTarget(null);
        }

        AttackableUnit GetLastHitTarget(IEnumerable<AttackableUnit> attackable)
        {
            if (attackable == null)
            {
                attackable = ObjectManager.Get<AttackableUnit>().Where(x => x.IsValidAutoRange() && !x.IsHero);
            }

            var availableMinionTargets = attackable.Where(x => x is Obj_AI_Base).Cast<Obj_AI_Base>()
                .Where(x => this.CanKillMinion(x));

            var bestMinionTarget = availableMinionTargets.OrderByDescending(x => x.MaxHealth)
                .ThenBy(x => this.HealthPrediction.GetAggroData(x)?.HasTurretAggro).ThenBy(x => x.Health)
                .FirstOrDefault();

            return bestMinionTarget;
        }

        AttackableUnit GetLaneClearTarget()
        {
            var attackable = ObjectManager.Get<AttackableUnit>().Where(x => x.IsValidAutoRange() && !x.IsHero);
   
            var attackableUnits = attackable as AttackableUnit[] ?? attackable.ToArray();

            IEnumerable<Obj_AI_Base> minions = attackableUnits.Where(x => x is Obj_AI_Base).Cast<Obj_AI_Base>()
                .OrderByDescending(x => x.MaxHealth);

            var minionTurretAggro = minions.FirstOrDefault(x => this.HealthPrediction.HasTurretAggro(x));

            if (minionTurretAggro != null)
            {
                var data = this.HealthPrediction.GetAggroData(minionTurretAggro);

                var timeToReach = this.TimeForAutoToReachTarget(minionTurretAggro) + 50;

                var predHealth1Auto = this.HealthPrediction.GetPredictedDamage(minionTurretAggro, timeToReach);

                var dmgauto = Player.GetAutoAttackDamage(minionTurretAggro);

                var turretDmg = data.LastTurretAttack.Sender.GetAutoAttackDamage(minionTurretAggro);

                //If it won't be dead already...
                if (predHealth1Auto > 0)
                {
                    if (Game.TickCount + timeToReach > Game.TickCount + data.LastTurretAttack.ETA)
                    {
                        if (dmgauto >= predHealth1Auto)
                        {
                            return minionTurretAggro;
                        }
                    }

                    //Our auto can reach sooner than the turret auto
                    else
                    {
                        if (Math.Ceiling(dmgauto - minionTurretAggro.Health) <= 0 || dmgauto > predHealth1Auto)
                        {
                            return minionTurretAggro;
                        }
                    }
                }

                var afterAutoHealth = predHealth1Auto - dmgauto;
                var afterTurretHealth = predHealth1Auto - turretDmg;

                if (afterAutoHealth > 0 && turretDmg >= afterAutoHealth)
                {
                    return null;
                }

                var numautos = this.NumberOfAutoAttacksInTime(
                    Player,
                    minionTurretAggro,
                    data.TimeUntilNextTurretAttack);

                var tdmg = dmgauto * numautos;

                var hNextTurretShot =
                    this.HealthPrediction.GetPredictedDamage(minionTurretAggro, data.TimeUntilNextTurretAttack - 100);

                if (tdmg >= minionTurretAggro.Health || tdmg >= hNextTurretShot)
                {
                    return minionTurretAggro;
                }

                return null;
            }


            //Killable
            AttackableUnit killableMinion = minions.FirstOrDefault(x => CanKillMinion(x));

            if (killableMinion != null)
            {
                return killableMinion;
            }

            var waitableMinion = minions.Any(this.ShouldWaitMinion);
            if (waitableMinion)
            {
                return null;
            }

            var structure = this.GetStructureTarget(attackableUnits);

            if (structure != null)
            {
                return structure;
            }

            foreach (var minion in minions.OrderBy(x => Math.Ceiling(this.GetPredictedHealth(x) / Player.GetAutoAttackDamage(x))))
            {
                var predHealth = this.GetPredictedHealth(minion) - 1;

                var dmg = Player.GetAutoAttackDamage(minion);

                var data = this.HealthPrediction.GetAggroData(minion);

                //if our damage is enough to kill it
                if (dmg >= predHealth)
                {
                    return minion;
                }

                if (data != null && data.HasMinionAggro && data.IncomingAttacks?.Count >= 3 && predHealth > dmg)
                {
                    continue;
                }

                return minion;
            }

            //Heros
            var hero = this.GetHeroTarget();
            if (hero != null)
            {
                return hero;
            }

            return null;
        }

        //In mixed mode we prioritize killable units, then structures, then heros. If none are found, then we don't attack anything.
        AttackableUnit GetMixedModeTarget()
        {
            var attackable = ObjectManager.Get<AttackableUnit>().Where(x => x.IsValidAutoRange() && !x.IsHero);

            var attackableUnits = attackable as AttackableUnit[] ?? attackable.ToArray();

            var killable = this.GetLastHitTarget(attackableUnits);

            //Killable unit 
            if (killable != null)
            {
                return killable;
            }

            //Structures
            var structure = this.GetStructureTarget(attackableUnits);
            if (structure != null)
            {
                return structure;
            }

            //Heros
            var hero = this.GetHeroTarget();
            if (hero != null)
            {
                return hero;
            }

            return null;
        }

        //Gets a structure target based on the following order (Nexus, Turret, Inihibitor)
        AttackableUnit GetStructureTarget(IEnumerable<AttackableUnit> attackable)
        {
            //Nexus
            var attackableUnits = attackable as AttackableUnit[] ?? attackable.ToArray();
            var nexus = attackableUnits.Where(x => x.Type == GameObjectType.obj_HQ)
                .OrderBy(x => x.Distance(Player)).FirstOrDefault();
            if (nexus != null && nexus.IsValidAutoRange())
            {
                return nexus;
            }

            //Turret
            var turret = attackableUnits.Where(x => x is Obj_AI_Turret).OrderBy(x => x.Distance(Player))
                .FirstOrDefault();
            if (turret != null && turret.IsValidAutoRange())
            {
                return turret;
            }

            //Inhib
            var inhib = attackableUnits.Where(x => x.Type == GameObjectType.obj_BarracksDampener)
                .OrderBy(x => x.Distance(Player)).FirstOrDefault();
            if (inhib != null && inhib.IsValidAutoRange())
            {
                return inhib;
            }

            return null;
        }

        int TimeForAutoToReachTarget(AttackableUnit minion)
        {
            var dist = Player.Distance(minion) - Player.BoundingRadius - minion.BoundingRadius;
            var ms = Player.IsMelee ? int.MaxValue : Player.BasicAttack.MissileSpeed;
            var attackTravelTime = (dist / ms) * 1000f;
            var totalTime = (int) (this.AnimationTime + attackTravelTime + 70 + Game.Ping / 2);
            return totalTime;
        }

        bool CanKillMinion(Obj_AI_Base minion, int time = 0)
        {
            var rtime = time == 0 ? (this.TimeForAutoToReachTarget(minion)) : (time);
            var pred = this.GetPredictedHealth(minion, rtime);

            //The minions health will already be 0 by the time our auto attack reaches it, so no point attacking it...
            if (pred <= 0)
            {
                this.FireNonKillableMinion(minion);
                return false;
            }

            var dmg = Player.GetAutoAttackDamage(minion);

            var result = (dmg) - pred >= 0;

            return result;
        }

        int NumberOfAutoAttacksInTime(Obj_AI_Base sender, AttackableUnit minion, int time)
        {
            var basetimePerAuto = this.TimeForAutoToReachTarget(minion);

            var numberOfAutos = 0;
            var adjustedTime = 0;

            if (this.AttackReady)
            {
                numberOfAutos++;
                adjustedTime = time - basetimePerAuto;
            }

            var fullTimePerAuto = basetimePerAuto + sender.AttackDelay * 1000;
            var additionalAutos = (int)Math.Ceiling(adjustedTime / fullTimePerAuto);

            numberOfAutos += additionalAutos;

            return numberOfAutos;
        }

        float DamageDealtInTime(Obj_AI_Base sender, Obj_AI_Base minion, int time)
        {
            var autos = this.NumberOfAutoAttacksInTime(sender, minion, time);
            var dmg = autos * sender.GetAutoAttackDamage(minion);

            return (float) (autos * dmg);
        }

        int GetPredictedHealth(Obj_AI_Base minion, int time = 0)
        {
            var rtime = time == 0 ? this.TimeForAutoToReachTarget(minion) : time;
            return (int) Math.Ceiling(this.HealthPrediction.GetPrediction(minion, rtime));
        }

        bool ShouldWaitMinion(Obj_AI_Base minion)
        {
            var pred = this.GetPredictedHealth(minion, this.TimeForAutoToReachTarget(minion) + (int)(this.AttackCoolDownTime) + (int)(this.WindUpTime));
            return Player.GetAutoAttackDamage(minion) - pred >= 0;
        }

        public override void Dispose()
        {
            this.Config.Dispose();
            Obj_AI_Base.OnProcessAutoAttack -= this.ObjAiHeroOnProcessAutoAttack;
            Obj_AI_Base.OnProcessSpellCast -= Obj_AI_Base_OnProcessSpellCast;
            Game.OnUpdate -= this.Game_OnUpdate;
            SpellBook.OnStopCast -= SpellBook_OnStopCast;
            RenderManager.OnRender -= RenderManager_OnRender;
            this.Attached = false;
        }

        public override AttackableUnit GetTarget(OrbwalkerMode mode)
        {
            if (this.ForcedTarget != null && this.ForcedTarget.IsValidAutoRange())
            {
                return ForcedTarget;
            }

            if (mode != null)
            {
                return mode.GetTarget();
            }

            return null;
        }

        public override bool Move(Vector3 movePosition)
        {
            var preMoveArgs = this.FirePreMove(movePosition);

            if (!preMoveArgs.Cancel)
            {
                if (Player.IssueOrder(OrderType.MoveTo, preMoveArgs.MovePosition))
                {
                    return true;
                }
            }

            return false;
        }

        public override bool Attack(AttackableUnit target)
        {
            var preAttackargs = this.FirePreAttack(target);

            if (!preAttackargs.Cancel)
            {
                if (Player.IssueOrder(OrderType.AttackUnit, target))
                {
                    this.LastAttackCommandSentTime = Game.TickCount;
                    return true;
                }
            }

            return false;
        }
    }
}
