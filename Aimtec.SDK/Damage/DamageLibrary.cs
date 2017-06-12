namespace Aimtec.SDK.Damage
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    using Aimtec.SDK.Damage.JSON;
    using Aimtec.SDK.Extensions;

    using Newtonsoft.Json;

    using NLog;

    /// <summary>
    ///     Class DamageLibrary.
    /// </summary>
    internal static class DamageLibrary
    {
        #region Public Properties

        /// <summary>
        ///     Gets the damages.
        /// </summary>
        /// <value>The damages.</value>
        public static IReadOnlyDictionary<string, ChampionDamage> Damages { get; private set; }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        #endregion

        #region Methods

        /// <summary>
        ///     Gets the bonus spell damage.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="spellBonus">The spell bonus.</param>
        /// <param name="index">The index.</param>
        /// <returns>System.Double.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal static double GetBonusSpellDamage(
            this Obj_AI_Base source,
            Obj_AI_Base target,
            DamageSpellBonus spellBonus,
            int index)
        {
            var sourceScale = spellBonus.ScalingTarget == DamageScalingTarget.Source ? source : target;
            var percent = spellBonus.DamagePercentages?.Count > 0
                ? spellBonus.DamagePercentages[Math.Min(index, spellBonus.DamagePercentages.Count - 1)]
                : 0d;
            float origin;

            switch (spellBonus.ScalingType)
            {
                case DamageScalingType.BonusAttackPoints:
                    origin = sourceScale.FlatPhysicalDamageMod;
                    break;
                case DamageScalingType.AbilityPoints:
                    origin = sourceScale.TotalAbilityDamage;
                    break;
                case DamageScalingType.AttackPoints:
                    origin = sourceScale.TotalAttackDamage;
                    break;
                case DamageScalingType.MaxHealth:
                    origin = sourceScale.MaxHealth;
                    break;
                case DamageScalingType.CurrentHealth:
                    origin = sourceScale.Health;
                    break;
                case DamageScalingType.MissingHealth:
                    origin = sourceScale.MaxHealth - sourceScale.Health;
                    break;
                case DamageScalingType.BonusHealth: // TODO Replace with correct health
                    origin = ((Obj_AI_Hero) sourceScale).MaxHealth;
                    break;
                case DamageScalingType.Armor:
                    origin = sourceScale.Armor;
                    break;
                case DamageScalingType.MaxMana:
                    origin = sourceScale.MaxMana;
                    break;
                default: throw new ArgumentOutOfRangeException();
            }

            var dmg = origin * (percent > 0 || percent < 0
                ? (percent > 0 ? percent : 0)
                + (spellBonus.ScalePer100Ap > 0
                    ? Math.Abs(source.TotalAbilityDamage / 100) * spellBonus.ScalePer100Ap
                    : 0) + (spellBonus.ScalePer100BonusAd > 0
                    ? Math.Abs(source.FlatPhysicalDamageMod / 100) * spellBonus.ScalePer100BonusAd
                    : 0) + (spellBonus.ScalePer100Ad > 0
                    ? Math.Abs(source.TotalAttackDamage / 100) * spellBonus.ScalePer100Ad
                    : 0)
                : 0);

            if (target is Obj_AI_Minion && spellBonus.BonusDamageOnMinion?.Count > 0)
            {
                dmg += spellBonus.BonusDamageOnMinion[Math.Min(index, spellBonus.BonusDamageOnMinion.Count - 1)];
            }

            if (!string.IsNullOrEmpty(spellBonus.ScalingBuff))
            {
                var buffCount = (spellBonus.ScalingBuffTarget == DamageScalingTarget.Source ? source : target)
                    .GetBuffCount(spellBonus.ScalingBuff);
                dmg = buffCount > 0 ? dmg * (buffCount + spellBonus.ScalingBuffOffset) : 0d;
            }

            if (dmg <= 0)
            {
                return dmg;
            }

            if (spellBonus.MinDamage?.Count > 0)
            {
                dmg = Math.Max(dmg, spellBonus.MinDamage[Math.Min(index, spellBonus.MinDamage.Count - 1)]);
            }

            if (target is Obj_AI_Minion && spellBonus.MaxDamageOnMinion?.Count > 0)
            {
                dmg = Math.Min(
                    dmg,
                    spellBonus.MaxDamageOnMinion[Math.Min(index, spellBonus.MaxDamageOnMinion.Count - 1)]);
            }

            return dmg;
        }

        /// <summary>
        ///     Loads the damages.
        /// </summary>
        internal static void LoadDamages()
        {
            Logger.Debug(
                "Embedded Resources: " + string.Join(
                    " | ",
                    Assembly.GetExecutingAssembly().GetManifestResourceNames()));

            try
            {
                // Todo makes this load based on game version
                using (var stream = Assembly.GetExecutingAssembly()
                                            .GetManifestResourceStream("Aimtec.SDK.Damage.Data.7.11.json"))
                {
                    if (stream == null)
                    {
                        Logger.Error($"Could not load the damage library. {nameof(stream)} was null.");
                        return;
                    }

                    using (var streamReader = new StreamReader(stream))
                    {
                        Damages =
                            JsonConvert.DeserializeObject<Dictionary<string, ChampionDamage>>(streamReader.ReadToEnd());
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e, "Could not load damages. Subsequent Damage API calls will return 0.");

                // Create empty damages to supress errors
                Damages = new Dictionary<string, ChampionDamage>();
            }
        }

        #endregion
    }
}