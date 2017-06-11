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

    internal class OrbwalkingImpl : IOrbwalker
    {
        public OrbwalkingImpl()
        {
            Obj_AI_Base.OnProcessAutoAttack += this.ObjAiHeroOnProcessAutoAttack;
            Obj_AI_Base.OnProcessSpellCast += this.ObjAiBaseOnOnProcessSpellCast;
            Game.OnUpdate += this.Game_OnUpdate;
            GameObject.OnCreate += this.GameObject_OnCreate;
            this.CreateMenu();
        }

        private static Obj_AI_Hero Player => ObjectManager.GetLocalPlayer();

        // TODO this completely breaks the modular design, Orbwalker and health prediction shouldnt be tightly coupled!
        private HealthPredictionImplB HealthPrediction { get; } = new HealthPredictionImplB();

        private Menu config;

        private List<CustomMode> CustomModes { get; } = new List<CustomMode>();

        private AttackableUnit LastTarget { get; set; }

        public float AnimationTime => Player.AttackCastDelay * 1000;

        public float WindUpTime => Player.AttackDelay * 1000;

        protected bool AttackReady => (Game.TickCount + Game.Ping / 2) - this.ServerAttackDetectionTick
                                      >= this.WindUpTime;

        //if we can move without cancelling the autoattack using the time the server processed our auto attack
        private bool CanMoveServer => Game.TickCount + Game.Ping / 2
                                      >= this.ServerAttackDetectionTick + this.AnimationTime + this.ExtraWindUp;

        //if we can move without cancelling the autoattack using the time the we actually sent out the auto attack
        protected bool CanMoveLocal => Game.TickCount
                                       >= this.LastAttackCommandSentTime + this.AnimationTime + this.ExtraWindUp;

        public bool CanMove => Player.Distance(Game.CursorPos) > this.HoldPositionRadius &&
            (NoCancelChamps.Contains(Player.ChampionName) || this.CanMoveServer && this.CanMoveLocal);

        public bool CanAttack => (Game.TickCount + Game.Ping / 2) - this.ServerAttackDetectionTick >= this.WindUpTime;

        public bool DisableAttacks { get; set; }

        public bool DisableMove { get; set; }

        //Events
        public event EventHandler<PreAttackEventArgs> PreAttack;

        public event EventHandler<PostAttackEventArgs> PostAttack;

        public event EventHandler<PreMoveEventArgs> PreMove;


        //Menu Getters
        protected int HoldPositionRadius => this.config["holdPositionRadius"].Value;

        protected int ExtraWindUp => this.config["extraWindup"].Value;



        //Members
        private float ServerAttackDetectionTick { get; set; }

        private float LastAttackCommandSentTime { get; set; }

        public OrbwalkingMode Mode
        {
            get => this.GetCurrentMode();
            set { }
        }

        //Don't think we really needs this since we have an auto attack detection event
        private string[] attacks =
            {
                "caitlynheadshotmissile", "frostarrow", "garenslash2", "kennenmegaproc", "masteryidoublestrike",
                "quinnwenhanced", "renektonexecute", "renektonsuperexecute", "rengarnewpassivebuffdash", "trundleq",
                "xenzhaothrust", "xenzhaothrust2", "xenzhaothrust3", "viktorqbuff", "lucianpassiveshot"
            };

        //Don't think we really needs this since we have an auto attack detection event
        private string[] blacklistedAttacks =
            {
                "volleyattack", "volleyattackwithsound", "jarvanivcataclysmattack", "monkeykingdoubleattack",
                "shyvanadoubleattack", "shyvanadoubleattackdragon", "zyragraspingplantattack",
                "zyragraspingplantattack2", "zyragraspingplantattackfire", "zyragraspingplantattack2fire",
                "viktorpowertransfer", "sivirwattackbounce", "asheqattacknoonhit", "elisespiderlingbasicattack",
                "heimertyellowbasicattack", "heimertyellowbasicattack2", "heimertbluebasicattack",
                "annietibbersbasicattack", "annietibbersbasicattack2", "yorickdecayedghoulbasicattack",
                "yorickravenousghoulbasicattack", "yorickspectralghoulbasicattack", "malzaharvoidlingbasicattack",
                "malzaharvoidlingbasicattack2", "malzaharvoidlingbasicattack3", "kindredwolfbasicattack",
                "gravesautoattackrecoil"
            };


        private readonly string[] AttackResets =
            {
                "dariusnoxiantacticsonh", "fiorae", "garenq", "gravesmove", "hecarimrapidslash", "jaxempowertwo",
                "jaycehypercharge", "leonashieldofdaybreak", "luciane", "monkeykingdoubleattack",
                "mordekaisermaceofspades", "nasusq", "nautiluspiercinggaze", "netherblade", "gangplankqwrapper",
                "powerfist", "renektonpreexecute", "rengarq", "shyvanadoubleattack", "sivirw", "takedown",
                "talonnoxiandiplomacy", "trundletrollsmash", "vaynetumble", "vie", "volibearq", "xenzhaocombotarget",
                "yorickspectral", "reksaiq", "itemtitanichydracleave", "masochism", "illaoiw", "elisespiderw", "fiorae",
                "meditate", "sejuaninorthernwinds", "asheq", "camilleq", "camilleq2"
            };


        private static readonly string[] NoCancelChamps = { "Kalista" };

        private void CreateMenu()
        {
            this.config = new Menu("Orbwalker", "Orbwalker")
                              {
                                  new MenuSlider("holdPositionRadius", "Hold Radius", 50, 0, 400, true),
                                  new MenuSlider("extraWindup", "Additional Windup", 0, 0, 200, true),
                                  new MenuBool("noBlindAA", "No AA when Blind", true, true),
                              };
        }


        //Situations where it does not make sense to cast an auto attack (independent of target)
        protected bool ShouldNotAttack => this.BlindCheck;

        private bool BlindCheck => this.config["noBlindAA"].Enabled && !Player.ChampionName.Equals("Kalista")
            && !Player.ChampionName.Equals("Twitch")
            && Player.BuffManager.HasBuffOfType(BuffType.Blind);

        protected void Game_OnUpdate()
        {
            this.Orbwalker();
        }

        private void Orbwalker()
        {
            var activeMode = this.GetCurrentMode();

            switch (activeMode)
            {
                case OrbwalkingMode.None:
                    return;
                case OrbwalkingMode.Custom:
                    this.CustomModeOrbwalking();
                    break;
                default:
                    var target = this.GetTarget();
                    this.Orbwalk(target);
                    break;
            }
        }

        private void ObjAiBaseOnOnProcessSpellCast(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs args)
        {
            if (sender.IsMe)
            {
                if (this.IsReset(args.SpellData.Name))
                {
                    this.ResetAutoAttackTimer();
                }
            }
        }

        protected void ObjAiHeroOnProcessAutoAttack(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs args)
        {
            if (sender.IsMe)
            {
                this.ServerAttackDetectionTick = Game.TickCount - Game.Ping / 2;

                var targ = args.Target as AttackableUnit;
                if (targ != null)
                {
                    this.LastTarget = targ;
                }
            }
        }

        public void Orbwalk(AttackableUnit target, bool move = true, bool attack = true)
        {
            if (this.AttackReady && target != null && !this.ShouldNotAttack)
            {
                var args = this.FirePreAttack(target);

                if (!args.Cancel)
                {
                    Player.IssueOrder(OrderType.AttackUnit, target);
                    this.LastAttackCommandSentTime = Game.TickCount;
                }
            }

            if (this.CanMove)
            {
                var args = this.FirePreMove(Game.CursorPos);

                if (!args.Cancel)
                {
                    Player.IssueOrder(OrderType.MoveTo, Game.CursorPos);
                }
            }
        }


        private void GameObject_OnCreate(GameObject sender)
        {
            if (sender != null)
            {
                var mc = sender as MissileClient;

                if (mc != null && mc.SpellCaster.IsMe)
                {
                    var unit = mc.Target as AttackableUnit;
                    if (unit != null)
                    {
                        this.FirePostAttack(unit);
                    }
                }
            }
        }

        protected void CustomModeOrbwalking()
        {
            var activeMode = this.ActiveCustomMode;
            if (activeMode != null)
            {
                this.Orbwalk(null, activeMode.MovingEnabled, activeMode.AttackingEnabled);
            }
        }

        private PreAttackEventArgs FirePreAttack(AttackableUnit target)
        {
            var args = new PreAttackEventArgs { Target = target };

            this.PreAttack?.Invoke(Player, args);

            return args;
        }

        private PostAttackEventArgs FirePostAttack(AttackableUnit target)
        {
            var args = new PostAttackEventArgs { Target = target };

            this.PostAttack?.Invoke(Player, args);

            return args;
        }

        private PreMoveEventArgs FirePreMove(Vector3 position)
        {
            var args = new PreMoveEventArgs { MovePosition = position };

            this.PreMove?.Invoke(Player, args);

            return args;
        }

        private OrbwalkingMode GetCurrentMode()
        {
            if (GlobalKeys.ComboKey.Active)
            {
                return OrbwalkingMode.Combo;
            }

            if (GlobalKeys.MixedKey.Active)
            {
                return OrbwalkingMode.Mixed;
            }

            if (GlobalKeys.WaveClearKey.Active)
            {
                return OrbwalkingMode.Laneclear;
            }

            if (GlobalKeys.LastHitKey.Active)
            {
                return OrbwalkingMode.Lasthit;
            }

            if (this.ActiveCustomMode != null)
            {
                return OrbwalkingMode.Custom;
            }

            return OrbwalkingMode.None;
        }

        private string GetCurrentModeString()
        {
            if (GlobalKeys.ComboKey.Active)
            {
                return "Combo";
            }

            if (GlobalKeys.MixedKey.Active)
            {
                return "Mixed";
            }

            if (GlobalKeys.WaveClearKey.Active)
            {
                return "Laneclear";
            }

            if (GlobalKeys.LastHitKey.Active)
            {
                return "Lasthit";
            }


            var active = this.ActiveCustomMode;
            if (active != null)
            {
                return active.Name;
            }

            return "None";
        }

        public CustomMode ActiveCustomMode
        {
            get
            {
                var activeCustomMode = this.CustomModes.Find(x => x.Active);
                if (activeCustomMode != null)
                {
                    return activeCustomMode;
                }

                return null;
            }
        }

        public AttackableUnit GetTarget()
        {
            var mode = this.GetCurrentMode();

            if (mode == OrbwalkingMode.Combo)
            {
                var targ = this.GetHeroTarget();

                if (targ != null)
                {
                    return targ;
                }
            }

            else if (mode == OrbwalkingMode.Mixed)
            {
                return this.GetMixedModeTarget();
            }

            else if (mode == OrbwalkingMode.Laneclear)
            {
                return this.GetWaveClearTarget();
            }

            else if (mode == OrbwalkingMode.Lasthit)
            {
                return this.GetLastHitTarget();
            }

            return null;
        }

        public void ResetAutoAttackTimer()
        {
            this.ServerAttackDetectionTick = 0;
        }

        public void AddToMenu(IMenu menu)
        {
            menu.Add(this.config);
        }

        public void ForceTarget(AttackableUnit unit)
        {

        }

        public bool IsSimpleAttack(Obj_AI_BaseMissileClientDataEventArgs args)
        {
            return !this.AttackResets.Contains(args.SpellData.Name);
        }

        public bool IsAttack(Obj_AI_BaseMissileClientDataEventArgs args)
        {
            return true;
        }

        public bool IsReset(string missileName)
        {
            var missileNameLc = missileName.ToLower();
            return this.AttackResets.Contains(missileNameLc);
        }

        AttackableUnit GetHeroTarget()
        {
            return TargetSelector.Implementation.GetOrbwalkingTarget();
        }

        AttackableUnit GetLastHitTarget(IEnumerable<AttackableUnit> attackable = null)
        {
            if (attackable == null)
            {
                attackable = ObjectManager.Get<AttackableUnit>().Where(x => !x.IsHero && x.IsValidAutoRange());
            }

            var availableMinionTargets = attackable.Where(x => x is Obj_AI_Base).Cast<Obj_AI_Base>()
                .Where(x => this.CanKillMinion(x));

            var bestMinionTarget = availableMinionTargets.OrderByDescending(x => x.MaxHealth)
                .ThenBy(x => this.HealthPrediction.GetAggroData(x).HasTurretAggro).ThenBy(x => x.Health)
                .FirstOrDefault();

            return bestMinionTarget;
        }

        AttackableUnit GetWaveClearTarget()
        {
            var attackable = ObjectManager.Get<AttackableUnit>().Where(x => !x.IsHero && x.IsValidAutoRange());

            var attackableUnits = attackable as AttackableUnit[] ?? attackable.ToArray();

            IEnumerable<Obj_AI_Base> minions = attackableUnits.Where(x => x is Obj_AI_Base).Cast<Obj_AI_Base>()
                .OrderByDescending(x => x.MaxHealth).ThenBy(x => x.NetworkId == this.LastTarget?.NetworkId);

            var minionTurretAggro = minions.FirstOrDefault(x => this.HealthPrediction.GetAggroData(x).HasTurretAggro);

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
                        if (dmgauto > minionTurretAggro.Health || dmgauto > predHealth1Auto)
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
            AttackableUnit killableMinion = minions.FirstOrDefault(x => this.CanKillMinion(x));

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

            foreach (var minion in minions.OrderByDescending(x => Math.Ceiling(this.GetPredictedHealth(x) / Player.GetAutoAttackDamage(x))).ThenBy(x => x.NetworkId == this.LastTarget?.NetworkId))
            {
                var predHealth = this.GetPredictedHealth(minion) - 1;

                var dmg = Player.GetAutoAttackDamage(minion);

                var data = this.HealthPrediction.GetAggroData(minion);

                //if our damage is enough to kill it
                if (dmg >= predHealth)
                {
                    return minion;
                }

                if (data != null && data.HasMinionAggro && data.IncomingAttacks.Count >= 3 && predHealth > dmg)
                {
                    continue;
                }

                var autos = Math.Ceiling(predHealth / Player.GetAutoAttackDamage(minion));
  
                if (autos >= 2)
                {
                    return minion;
                } 
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
            var attackable = ObjectManager.Get<AttackableUnit>().Where(x => !x.IsHero && x.IsValidAutoRange());

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
            var ms = Player.BasicAttack.MissileSpeed * 1000;

            return (int)(this.AnimationTime + (Game.Ping / 2f) + (int)Math.Max(0, dist / ms));
        }

        bool CanKillMinion(Obj_AI_Base minion, int time = 0)
        {
            var rtime = time == 0 ? (this.TimeForAutoToReachTarget(minion)) : (time);

            var pred = this.GetPredictedHealth(minion, rtime);

            //The minions health will already be 0 by the time our auto attack reaches it, so no point attacking it...
            if (pred <= 0)
            {
                return false;
            }

            return Player.GetAutoAttackDamage(minion) - pred >= 0;
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
            return (int)this.HealthPrediction.GetPrediction(minion, rtime);
        }

        bool ShouldWaitMinion(Obj_AI_Base minion)
        {
            var pred = this.GetPredictedHealth(minion, this.TimeForAutoToReachTarget(minion) + (int)(this.WindUpTime));
            return Player.GetAutoAttackDamage(minion) - pred >= 0;
        }

        public void Dispose()
        {
            
        }

        //Adding a custom mode that will be in this orbwalker's menu instance because it is not using a standard key
        public void AddCustomMode(string modeName, KeyCode modeKey, bool movingEnabled = true, bool attackingEnabled = false)
        {
            if (this.CustomModes.Any(x => x.Name == modeName) || Enum.GetNames(typeof(OrbwalkingMode)).Contains(modeName))
            {
                throw new Exception($"Unable to add a custom mode with the name \"{modeName}\" because it already exists.");
            }

            var mode = new CustomMode(modeName, modeKey, movingEnabled, attackingEnabled);

            this.CustomModes.Add(mode);
            this.config.Add(mode.MenuItem);
        }

        //Adding a custom mode that will be using a key from the Global Keys
        public void AddCustomMode(string modeName, GlobalKeys.Key key, bool movingEnabled = true, bool attackingEnabled = false)
        {
            if (this.CustomModes.Any(x => x.Name == modeName) || Enum.GetNames(typeof(OrbwalkingMode)).Contains(modeName))
            {
                throw new Exception($"Unable to add a custom mode with the name \"{modeName}\" because it already exists.");
            }

            var mode = new CustomMode(modeName, key, movingEnabled, attackingEnabled);

            this.CustomModes.Add(mode);
        }

        // TODO Let developer customize what to do in regards to attacking and moving, instead of just letting them
        // disable/enable moving and attacking.
        public class CustomMode
        {
            //Using a GlobalKey
            public CustomMode(string name, GlobalKeys.Key key, bool movingEnabled = true, bool attackingEnabled = false)
            {
                this.Name = name;
                this.MenuItem = key.KeyBindItem;
                this.AttackingEnabled = attackingEnabled;
                this.MovingEnabled = movingEnabled;

                key.Activate();
            }

            //Using a custom key
            public CustomMode(string name, KeyCode key, bool movingEnabled = true, bool attackingEnabled = false)
            {
                this.Name = name;
                this.MenuItem = new MenuKeyBind(name, name, key, KeybindType.Press);
                this.AttackingEnabled = attackingEnabled;
                this.MovingEnabled = movingEnabled;
            }

            public bool AttackingEnabled;
            public bool MovingEnabled;

            public string Name;
            public MenuKeyBind MenuItem;
            public bool Active => this.MenuItem.Enabled;
        }
    }
}
