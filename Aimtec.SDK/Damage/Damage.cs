namespace Aimtec.SDK.Damage
{
    using System;
    using System.Collections.Generic;

    using Aimtec.SDK.Damage.JSON;
    using Aimtec.SDK.Extensions;
    using Aimtec.SDK.Menu;
    using Aimtec.SDK.Menu.Components;

    /// <summary>
    ///     Class Damage.
    /// </summary>
    public static class Damage
    {
        #region Properties

        private static Dictionary<string, Dictionary<SpellSlot, DamageSpell>> DamageLibrary { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Calculates the damage.
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="target">Target.</param>
        /// <param name="damageType">Type of the damage.</param>
        /// <param name="damage">The damage.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="from" /> or <paramref name="target" /> was null.
        /// </exception>
        public static float CalculateDamage(
            this Obj_AI_Base from,
            Obj_AI_Base target,
            DamageType damageType,
            float damage)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from));
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            var percentMod = 1f;
            float baseResistance;
            float bonusResistance;
            float reductionFlat;
            float reductionPercent;
            float penetrationFlat;
            float penetrationPercent;
            float bonusPenetrationPercent;

            switch (damageType)
            {
                case DamageType.Physical:
                    baseResistance = target.Armor;
                    bonusResistance = target.BonusArmor;

                    reductionFlat = 0f; // TODO not in api
                    reductionPercent = 0f; // TODO not in api
                    penetrationFlat = from.FlatArmorPenetration;
                    penetrationPercent = from.PercentArmorPenetration;
                    bonusPenetrationPercent = from.PercentBonusArmorPenetration;

                    //todo check minions
                    break;
                case DamageType.Magical:
                    baseResistance = target.SpellBlock;
                    bonusResistance = target.BonusSpellBlock;

                    reductionFlat = from.FlatMagicReduction;
                    reductionPercent = from.PercentMagicReduction;
                    penetrationFlat = from.FlatMagicPenetration;
                    penetrationPercent = from.PercentMagicPenetration;
                    bonusPenetrationPercent = from.PercentBonusMagicPenetration;
                    break;
                case DamageType.True: return damage;
                case DamageType.Mixed:
                    throw new NotSupportedException(
                        "CalculateDamage does not support mixed damage. Instead, use Damage.CalculateMixedDamage");
                default: throw new ArgumentOutOfRangeException(nameof(damageType), damageType, null);
            }

            var resistance = baseResistance + bonusResistance;

            if (resistance > 0)
            {
                var basePercent = baseResistance / resistance;
                var bonusPercent = 1 - basePercent;

                baseResistance -= reductionFlat * basePercent;
                bonusResistance -= reductionFlat * bonusPercent;

                resistance = baseResistance + bonusResistance;

                if (resistance > 0)
                {
                    baseResistance *= 1f - reductionPercent;
                    bonusResistance *= 1f - reductionPercent;
                    baseResistance *= 1f - penetrationPercent;
                    bonusResistance *= 1f - penetrationPercent;
                    bonusResistance *= 1f - bonusPenetrationPercent;

                    resistance = baseResistance + bonusResistance;
                    resistance -= penetrationFlat;
                }
            }

            if (resistance >= 0)
            {
                percentMod *= 100f / (100f + resistance);
            }
            else
            {
                percentMod *= 2f - 100f / (100f - resistance);
            }

            // TODO
            var percentReceived = 1f;
            var percentPassive = 1f;
            var flatPassive = 0f;
            var flatReceived = 0f;
            var otherDamageModifier = 0f;

            return Math.Max(
                percentReceived * percentPassive * percentMod * (damage + flatPassive) + flatReceived
                + otherDamageModifier,
                0f);
        }

        /// <summary>
        ///     Calculates the mixed damage.
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="target">The target.</param>
        /// <param name="physicalDamage">The physical damage.</param>
        /// <param name="magicalDamage">The magical damage.</param>
        /// <param name="trueDamage">The true damage.</param>
        /// <returns></returns>
        public static float CalculateMixedDamage(
            this Obj_AI_Base from,
            Obj_AI_Base target,
            float physicalDamage = 0f,
            float magicalDamage = 0f,
            float trueDamage = 0f)
        {
            return from.CalculateDamage(target, DamageType.Physical, physicalDamage)
                + from.CalculateDamage(target, DamageType.Magical, magicalDamage) + trueDamage;
        }

        /// <summary>
        /// Gets the automatic attack damage.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <returns>System.Single.</returns>
        /// <exception cref="ArgumentNullException">
        /// source
        /// or
        /// target
        /// </exception>
        public static float GetAutoAttackDamage(this Obj_AI_Base source, Obj_AI_Base target)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            double dmgPhysical = source.TotalAttackDamage;
            double dmgMagical = 0;
            double dmgPassive = 0;
            var dmgReduce = 1d;

            var hero = source as Obj_AI_Hero;
            var targetHero = target as Obj_AI_Hero;

            var isMixDmg = false;

            if (hero != null)
            {
                switch (hero.ChampionName)
                {
                    case "Kalista":
                        dmgPhysical *= 0.9;
                        break;
                    case "Jhin":
                        dmgPhysical += Math.Round(
                            (new[] { 2, 3, 4, 5, 6, 7, 8, 10, 12, 14, 16, 18, 20, 24, 28, 32, 36, 40 }[hero.Level - 1]
                                + (Math.Round((hero.Crit * 100 / 10) * 4))
                                + (Math.Round(((hero.AttackSpeedMod - 1) * 100 / 10) * 2.5))) / 100 * dmgPhysical);
                        break;
                    case "Corki":
                        dmgPhysical /= 2;
                        dmgMagical = dmgPhysical;
                        isMixDmg = true;
                        break;
                    case "Quinn":
                        if (target.HasBuff("quinnw"))
                        {
                            dmgPhysical += 10 + (5 * hero.Level)
                                + (0.14 + (0.02 * hero.Level)) * hero.TotalAttackDamage;
                        }
                        break;
                }

                // Serrated Dirk
                if (hero.HasBuff("Serrated"))
                {
                    if (!isMixDmg)
                    {
                        dmgPhysical += 15;
                    }
                    else
                    {
                        dmgPhysical += 7.5;
                        dmgMagical += 7.5;
                    }
                }
            }

            dmgPhysical = source.CalculateDamage(target, DamageType.Physical, (float) dmgPhysical);
            dmgMagical = source.CalculateDamage(target, DamageType.Magical, (float) dmgMagical);

            if (targetHero != null && targetHero.ChampionName == "Fizz")
            {
                dmgPhysical -= 4 + (2 * Math.Floor((targetHero.Level - 1) / 3d));
            }

            return (float) Math.Max(Math.Floor((dmgPhysical + dmgMagical) * dmgReduce + dmgPassive), 0);
        }

        /// <summary>
        ///     Gets the spell damage.
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="target">The target.</param>
        /// <param name="slot">The slot.</param>
        /// <param name="stage">The stage.</param>
        /// <returns>System.Single.</returns>
        public static float GetSpellDamage(
            this Obj_AI_Hero from,
            Obj_AI_Base target,
            SpellSlot slot,
            SpellStage stage = SpellStage.Default)
        {
            return 0f;
        }

        #endregion
    }
}