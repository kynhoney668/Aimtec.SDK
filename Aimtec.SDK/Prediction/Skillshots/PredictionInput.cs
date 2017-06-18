using System.Collections.Generic;

namespace Aimtec.SDK.Prediction
{
    using Aimtec.SDK.Prediction.Collision;
    using Aimtec.SDK.Prediction.Skillshots;

    /// <summary>
    ///     The input parameters to calculate skillshot prediction.
    /// </summary>
    public class PredictionInput
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PredictionInput" /> class.
        /// </summary>
        public PredictionInput()
        {
            this.From = ObjectManager.GetLocalPlayer().Position;
            this.RangeCheckFrom = this.From;
        }

        /// <summary>
        /// Gets or sets the unit.
        /// </summary>
        /// <value>The unit.</value>
        public Obj_AI_Base Unit { get; set; }
        /// <summary>
        ///     Gets or sets from.
        /// </summary>
        /// <value>
        ///     From.
        /// </value>
        public Vector3 From { get; set; }

        /// <summary>
        ///     Gets or sets the the position to check the range from.
        /// </summary>
        /// <value>
        ///     The position to check the range from.
        /// </value>
        public Vector3 RangeCheckFrom { get; set; }

        /// <summary>
        ///     Gets or sets the collision types.
        /// </summary>
        /// <value>
        ///     The collision types.
        /// </value>
        public CollisionableObjects CollisionObjects { get; set; }

        /// <summary>
        ///     Gets or sets the spell delay.
        /// </summary>
        /// <value>
        ///     The spell delay.
        /// </value>
        public float Delay { get; set; }

        /// <summary>
        ///     Gets or sets the spell width.
        /// </summary>
        /// <value>
        ///     The width.
        /// </value>
        public float Radius { get; set; }

        /// <summary>
        ///     Gets or sets the spell range.
        /// </summary>
        /// <value>
        ///     The range.
        /// </value>
        public float Range { get; set; }

        /// <summary>
        ///     Gets or sets the spell speed.
        /// </summary>
        /// <value>
        ///     The speed.
        /// </value>
        public float Speed { get; set; }

        /// <summary>
        ///     Gets or sets the cast type of the skill.
        /// </summary>
        /// <value>
        ///     The cast type of the skill.
        /// </value>
        public SkillType SkillType { get; set; }

        /// <summary>
        ///     Gets or sets the target.
        /// </summary>
        /// <value>
        ///     The target.
        /// </value>
        public Obj_AI_Base Target { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the spell is an area effect spell.
        /// </summary>
        /// <value><c>true</c> if the spell is an area effect spell; otherwise, <c>false</c>.</value>
        public bool AoE { get; set; }

    }
}