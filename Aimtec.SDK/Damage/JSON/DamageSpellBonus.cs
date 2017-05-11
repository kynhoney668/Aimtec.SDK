using System.Collections.Generic;

namespace Aimtec.SDK.Damage.JSON
{
    public class DamageSpellBonus
    {
        /// <summary>
        /// Gets or sets the bonus damage on minion.
        /// </summary>
        /// <value>
        /// The bonus damage on minion.
        /// </value>
        public List<double> BonusDamageOnMinion { get; set; }
        /// <summary>
        /// Gets or sets the damage percentages.
        /// </summary>
        /// <value>
        /// The damage percentages.
        /// </value>
        public List<double> DamagePercentages { get; set; }
        /// <summary>
        /// Gets or sets the type of the damage.
        /// </summary>
        /// <value>
        /// The type of the damage.
        /// </value>
        public DamageType DamageType { get; set; }
        /// <summary>
        /// Gets or sets the maximum damage on minion.
        /// </summary>
        /// <value>
        /// The maximum damage on minion.
        /// </value>
        public List<int> MaxDamageOnMinion { get; set; }
        /// <summary>
        /// Gets or sets the minimum damage.
        /// </summary>
        /// <value>
        /// The minimum damage.
        /// </value>
        public List<int> MinDamage { get; set; }
        /// <summary>
        /// Gets or sets the per hundred ad scale.
        /// </summary>
        /// <value>
        /// The per hundred ad scale.
        /// </value>
        public double PerHundredAdScale { get; set; }
        /// <summary>
        /// Gets or sets the per hundred bonus ad scale.
        /// </summary>
        /// <value>
        /// The per hundred bonus ad scale.
        /// </value>
        public double PerHundredBonusAdScale { get; set; }
        /// <summary>
        /// Gets or sets the per hundred ap scale.
        /// </summary>
        /// <value>
        /// The per hundred ap scale.
        /// </value>
        public double PerHundredApScale { get; set; }
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
        /// Gets or sets the scaling target.
        /// </summary>
        /// <value>
        /// The scaling target.
        /// </value>
        public ScalingTarget ScalingTarget { get; set; }
        /// <summary>
        /// Gets or sets the type of the scaling.
        /// </summary>
        /// <value>
        /// The type of the scaling.
        /// </value>
        public ScalingType ScalingType { get; set; }
    }
}
