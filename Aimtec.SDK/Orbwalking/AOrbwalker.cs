using System;
using Aimtec.SDK.Menu;
using System.Collections.Generic;
using NLog;
using System.Linq;
using Aimtec.SDK.Extensions;
using Aimtec.SDK.Menu.Config;
using Aimtec.SDK.Util;

namespace Aimtec.SDK.Orbwalking
{
    /// <summary>
    ///     AOrbwalker class
    /// </summary>
    public abstract class AOrbwalker : IOrbwalker
    {
        /// <summary>
        ///     The Player
        /// </summary>
        protected static Obj_AI_Hero Player => ObjectManager.GetLocalPlayer();

        /// <inheritdoc cref="IOrbwalker" />
        public string[] AttackResets { get => _attackResets; set => _attackResets = value; }

        private string[] _attackResets =
        {
                "dariusnoxiantacticsonh", "fiorae", "garenq", "gravesmove", "hecarimrapidslash", "jaxempowertwo",
                "jaycehypercharge", "leonashieldofdaybreak", "luciane", "monkeykingdoubleattack",
                "mordekaisermaceofspades", "nasusq", "nautiluspiercinggaze", "netherblade", "gangplankqwrapper",
                "powerfist", "renektonpreexecute", "rengarq", "shyvanadoubleattack", "sivirw", "takedown",
                "talonnoxiandiplomacy", "trundletrollsmash", "vaynetumble", "vie", "volibearq", "xenzhaocombotarget",
                "yorickspectral", "reksaiq", "itemtitanichydracleave", "masochism", "illaoiw", "elisespiderw", "fiorae",
                "meditate", "sejuaninorthernwinds", "asheq", "camilleq", "camilleq2"
        };

        /// <summary>
        ///     Names of champions which cannot cancel their auto attacks
        /// </summary>
        public string[] NoCancelChamps = { "Kalista" };

        /// <inheritdoc cref="IOrbwalker" />
        public OrbwalkerMode Combo { get; set; }

        /// <inheritdoc cref="IOrbwalker" />
        public OrbwalkerMode LaneClear { get; set; }

        /// <inheritdoc cref="IOrbwalker" />
        public OrbwalkerMode LastHit { get; set; }

        /// <inheritdoc cref="IOrbwalker" />
        public OrbwalkerMode Mixed { get; set; }

        /// <summary>
        ///     List of the Orbwalker Modes that are currently active for this instance
        /// </summary>
        protected List<OrbwalkerMode> OrbwalkerModes { get; } = new List<OrbwalkerMode>();

        /// <summary>
        ///     Gets the logger instance for this class
        /// </summary>
        protected Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///     The Orbwalker Menu
        /// </summary>
        protected Menu.Menu Config;

        /// <inheritdoc cref="IOrbwalker" />
        public abstract float WindUpTime { get; }

        /// <inheritdoc cref="IOrbwalker" />
        public abstract bool IsWindingUp { get; }

        /// <inheritdoc cref="IOrbwalker" />
        public virtual bool AttackingEnabled { get; set; } = true;

        /// <inheritdoc cref="IOrbwalker" />
        public virtual bool MovingEnabled { get; set; } = true;

        /// <inheritdoc cref="IOrbwalker" />
        public virtual OrbwalkingMode Mode
        {
            get
            {
                var activeMode = GetActiveMode();

                if (activeMode == null)
                {
                    return OrbwalkingMode.None;
                }

                if (activeMode.Name == "Combo")
                {
                    return OrbwalkingMode.Combo;
                }

                if (activeMode.Name == "Laneclear")
                {
                    return OrbwalkingMode.Laneclear;
                }

                if (activeMode.Name == "Mixed")
                {
                    return OrbwalkingMode.Mixed;
                }

                if (activeMode.Name == "Lasthit")
                {
                    return OrbwalkingMode.Lasthit;
                }

                return OrbwalkingMode.Custom;
            }
        }

        /// <inheritdoc cref="IOrbwalker" />
        public virtual string ModeName
        {
            get
            {
                var active = GetActiveMode();
                if (active == null)
                {
                    return "None";
                }

                return active.Name;
            }
        }

        /// <inheritdoc cref="IOrbwalker" />
        public event EventHandler<PreAttackEventArgs> PreAttack;

        /// <inheritdoc cref="IOrbwalker" />
        public event EventHandler<PostAttackEventArgs> PostAttack;

        /// <inheritdoc cref="IOrbwalker" />
        public event EventHandler<NonKillableMinionEventArgs> OnNonKillableMinion;

        /// <inheritdoc cref="IOrbwalker" />
        public event EventHandler<PreMoveEventArgs> PreMove;

        /// <inheritdoc cref="IOrbwalker" />
        public abstract void Attach(IMenu menu);

        /// <inheritdoc cref="IOrbwalker" />
        public abstract bool CanAttack();

        /// <inheritdoc cref="IOrbwalker" />
        public abstract bool CanMove();

        /// <inheritdoc cref="IOrbwalker" />
        public abstract void Dispose();

        /// <inheritdoc cref="IOrbwalker" />
        public abstract void ForceTarget(AttackableUnit unit);

        /// <inheritdoc cref="IOrbwalker" />
        public abstract void Orbwalk();


        /// <inheritdoc cref="IOrbwalker" />
        public abstract void ResetAutoAttackTimer();

        /// <inheritdoc cref="IOrbwalker" />
        public virtual bool IsReset(string missileName)
        {
            var missileNameLc = missileName.ToLower();
            return AttackResets.Contains(missileNameLc);
        }

        /// <inheritdoc cref="IOrbwalker" />
        public OrbwalkerMode GetActiveMode()
        {
            return this.OrbwalkerModes.FirstOrDefault(x => x.Active);
        }

        /// <inheritdoc cref="IOrbwalker" />
        public void AddMode(OrbwalkerMode mode)
        {
            Logger.Info($"Adding mode {mode.Name}");
            if (this.OrbwalkerModes.Any(x => x.Name == mode.Name))
            {
                Logger.Error($"Unable to add mode with the name \"{mode.Name}\" because it already exists.");
                return;
            }

            mode.ParentInstance = this;

            this.OrbwalkerModes.Add(mode);

            if (!mode.UsingGlobalKey)
            {
                this.Config.Add(mode.MenuItem);
            }
        }

        /// <inheritdoc cref="IOrbwalker" />
        public OrbwalkerMode DuplicateMode(OrbwalkerMode mode, string newName, KeyCode key)
        {
            OrbwalkerMode newMode = new OrbwalkerMode(newName, key, mode.GetTargetImplementation, mode.ModeBehaviour);
            this.AddMode(newMode);
            return newMode;
        }

        /// <inheritdoc cref="IOrbwalker" />
        public OrbwalkerMode DuplicateMode(OrbwalkerMode mode, string newName, GlobalKey key)
        {
            OrbwalkerMode newMode = new OrbwalkerMode(newName, key, mode.GetTargetImplementation, mode.ModeBehaviour);
            this.AddMode(newMode);
            return newMode;
        }

        /// <summary>
        ///     Fires the Pre Attack Event
        /// </summary>
        protected PreAttackEventArgs FirePreAttack(AttackableUnit target)
        {
            var args = new PreAttackEventArgs { Target = target };

            this.PreAttack?.Invoke(Player, args);

            return args;
        }

        /// <summary>
        ///     Fires the Post Attack Event
        /// </summary>
        protected PostAttackEventArgs FirePostAttack(AttackableUnit target)
        {
            var args = new PostAttackEventArgs { Target = target };

            this.PostAttack?.Invoke(Player, args);

            return args;
        }

        /// <summary>
        ///     Fires the OnNonKillableMinion event
        /// </summary>
        protected NonKillableMinionEventArgs FireNonKillableMinion(AttackableUnit target)
        {
            var args = new NonKillableMinionEventArgs { Target = target };

            this.OnNonKillableMinion?.Invoke(Player, args);

            return args;
        }

        /// <summary>
        ///     Fires the Pre Move Event
        /// </summary>
        protected PreMoveEventArgs FirePreMove(Vector3 position)
        {
            var args = new PreMoveEventArgs { MovePosition = position };

            this.PreMove?.Invoke(Player, args);

            return args;
        }


        /// <inheritdoc cref="IOrbwalker" />
        public abstract bool Move(Vector3 movePosition);

        /// <inheritdoc cref="IOrbwalker" />
        public abstract bool Attack(AttackableUnit target);

        /// <inheritdoc cref="IOrbwalker" />
        public abstract AttackableUnit GetTarget(OrbwalkerMode mode);

        /// <inheritdoc cref="IOrbwalker" />
        public virtual AttackableUnit GetTarget()
        {
            return GetTarget(GetActiveMode());
        }
    }
}
