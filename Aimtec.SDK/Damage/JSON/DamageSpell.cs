namespace Aimtec.SDK.Damage.JSON
{
    public class DamageSpell
    {
        /// <summary>
        /// Gets or sets the stage.
        /// </summary>
        /// <value>
        /// The stage.
        /// </value>
        public SpellStage Stage { get; set; }

        /// <summary>
        /// Gets or sets the spell data.
        /// </summary>
        /// <value>
        /// The spell data.
        /// </value>
        public DamageSpellData SpellData { get; set; }
    }
}
