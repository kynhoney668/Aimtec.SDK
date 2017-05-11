using System.Collections.Generic;

namespace Aimtec.SDK.Damage.JSON
{
    public class DamageSpellData
    {
        /// <summary>
        /// Gets or sets the bonus damage on minion.
        /// </summary>
        /// <value>
        /// The bonus damage on minion.
        /// </value>
        public List<double> BonusDamageOnMinion { get; set; }
        /// <summary>
        /// Gets or sets the bonus damages.
        /// </summary>
        /// <value>
        /// The bonus damages.
        /// </value>
        public List<DamageSpellBonus> BonusDamages { get; set; }
        /// <summary>
        /// Gets or sets the damages.
        /// </summary>
        /// <value>
        /// The damages.
        /// </value>
        public List<double> Damages { get; set;}
        /// <summary>
        /// Gets or sets the damage per level.
        /// </summary>
        /// <value>
        /// The damage per level.
        /// </value>
        public List<double> DamagePerLevel { get; set; }
        /// <summary>
        /// Gets or sets the type of the damage.
        /// </summary>
        /// <value>
        /// The type of the damage.
        /// </value>
        public DamageType DamageType { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [apply on hit].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [apply on hit]; otherwise, <c>false</c>.
        /// </value>
        public bool ApplyOnHit { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this spell modifies other damages.
        /// </summary>
        /// <value>
        ///   <c>true</c> if modified damage; otherwise, <c>false</c>.
        /// </value>
        public bool ModifiedDamage { get; set; }
        /// <summary>
        /// Gets or sets the maximum damage on minion.
        /// </summary>
        /// <value>
        /// The maximum damage on minion.
        /// </value>
        public List<int> MaxDamageOnMinion { get; set; }
        /// <summary>
        /// Gets or sets the scale per target missing health.
        /// </summary>
        /// <value>
        /// The scale per target missing health.
        /// </value>
        public double ScalePerTargetMissingHealth { get; set; }
        /// <summary>
        /// Gets or sets the scale slot.
        /// </summary>
        /// <value>
        /// The scale slot.
        /// </value>
        public SpellSlot ScaleSlot { get; set; }
        /// <summary>
        /// Gets or sets the scaling buff.
        /// </summary>
        /// <value>
        /// The scaling buff.
        /// </value>
        public string ScalingBuff { get; set; }
        /// <summary>
        /// Gets or sets the scaling buff offset.
        /// </summary>
        /// <value>
        /// The scaling buff offset.
        /// </value>
        public int ScalingBuffOffset { get; set; }
        /// <summary>
        /// Gets or sets the scaling buff target.
        /// </summary>
        /// <value>
        /// The scaling buff target.
        /// </value>
        public ScalingTarget ScalingBuffTarget { get; set; }
        /// <summary>
        /// Gets or sets the type of the effect.
        /// </summary>
        /// <value>
        /// The type of the effect.
        /// </value>
        public SpellEffect EffectType { get; set; }
    }
}
