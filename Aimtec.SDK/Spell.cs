namespace Aimtec.SDK
{
    using Aimtec.SDK.Prediction;
    using Aimtec.SDK.Prediction.Collision;
    using Aimtec.SDK.Prediction.Skillshots;

    using NLog;

    /// <summary>
    ///     Class Spell.
    /// </summary>
    public class Spell
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Spell" /> class.
        /// </summary>
        /// <param name="slot">The slot.</param>
        public Spell(SpellSlot slot)
        {
            this.Slot = slot;

            Logger.Debug("{0} Spell Created", slot);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Spell" /> class.
        /// </summary>
        /// <param name="slot">The slot.</param>
        /// <param name="range">The range.</param>
        public Spell(SpellSlot slot, float range)
            : this(slot)
        {
            this.Range = range;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the delay.
        /// </summary>
        /// <value>The delay.</value>
        public float Delay { get; set; }

        /// <summary>
        ///     Gets or sets the hit chance.
        /// </summary>
        /// <value>The hit chance.</value>
        public HitChance HitChance { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is skill shot.
        /// </summary>
        /// <value><c>true</c> if this instance is skill shot; otherwise, <c>false</c>.</value>
        public bool IsSkillShot { get; set; }

        /// <summary>
        ///     Gets or sets the range.
        /// </summary>
        /// <value>The range.</value>
        public float Range { get; set; } = float.MaxValue;

        /// <summary>
        ///     Gets a value indicating whether this <see cref="Spell" /> is ready.
        /// </summary>
        /// <value><c>true</c> if ready; otherwise, <c>false</c>.</value>
        public bool Ready => Player.SpellBook.GetSpellState(this.Slot) == SpellState.Ready;

        /// <summary>
        ///     Gets or sets the slot.
        /// </summary>
        /// <value>The slot.</value>
        public SpellSlot Slot { get; set; }

        /// <summary>
        ///     Gets or sets the speed.
        /// </summary>
        /// <value>The speed.</value>
        public float Speed { get; set; }

        /// <summary>
        ///     Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        public SkillType Type { get; set; }

        /// <summary>
        ///     Gets or sets the width.
        /// </summary>
        /// <value>The width.</value>
        public float Width { get; set; }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///     Gets the player.
        /// </summary>
        /// <value>The player.</value>
        private static Obj_AI_Hero Player => ObjectManager.GetLocalPlayer();

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Casts the specified target.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns><c>true</c> if the spell was casted, <c>false</c> otherwise.</returns>
        public bool Cast(Obj_AI_Base target)
        {
            if (!this.IsSkillShot)
            {
                return Player.SpellBook.CastSpell(this.Slot, target);
            }

            var prediction = Prediction.Skillshots.Prediction.Instance.GetPrediction(this.GetPredictionInput(target));

            return prediction.HitChance >= this.HitChance
                && Player.SpellBook.CastSpell(this.Slot, prediction.CastPosition);
        }

        /// <summary>
        ///     Casts this instance.
        /// </summary>
        /// <returns><c>true</c> if the spell was casted, <c>false</c> otherwise.</returns>
        public bool Cast()
        {
            if (this.IsSkillShot)
            {
                Logger.Warn("{0} is a skillshot, but casted like a self-activated ability.", this.Slot);
            }

            return Player.SpellBook.CastSpell(this.Slot);
        }

        /// <summary>
        ///     Casts the specified position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool Cast(Vector2 position)
        {
            return Player.SpellBook.CastSpell(
                this.Slot,
                new Vector3(position.X, NavMesh.GetHeightForWorld(position.X, position.Y), position.Y));
        }

        /// <summary>
        ///     Casts the specified position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool Cast(Vector3 position)
        {
            return Player.SpellBook.CastSpell(this.Slot, position);
        }

        /// <summary>
        ///     Sets the skillshot.
        /// </summary>
        /// <param name="delay">The delay.</param>
        /// <param name="width">The width.</param>
        /// <param name="speed">The speed.</param>
        /// <param name="type">The type.</param>
        /// <param name="hitchance">The hitchance.</param>
        public void SetSkillshot(
            float delay,
            float width,
            float speed,
            SkillType type,
            HitChance hitchance = HitChance.Low)
        {
            this.Delay = delay;
            this.Width = width;
            this.Speed = speed;
            this.Type = type;

            this.IsSkillShot = true;
            this.HitChance = hitchance;

            Logger.Debug(
                "{0} Set as SkillShot -> Range: {1}, Delay: {2},  Width: {3}, Speed: {4}, Type: {5}, MinHitChance: {6}",
                this.Slot,
                this.Range,
                delay,
                width,
                speed,
                type,
                hitchance);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Gets the prediction input.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns>PredictionInput.</returns>
        private PredictionInput GetPredictionInput(Obj_AI_Base target)
        {
            return new PredictionInput()
            {
                CollisionObjects = CollisionableObjects.Minions,
                Delay = this.Delay,
                Radius = this.Width,
                Speed = this.Speed,
                Range = this.Range,
                Target = target,
                Unit = Player
            };
        }

        #endregion
    }
}