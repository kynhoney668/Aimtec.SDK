using System;
using System.Collections.Generic;
using Aimtec.SDK.Damage.JSON;

namespace Aimtec.SDK.Damage
{
    public static class Damage
    {
        private static Dictionary<string, Dictionary<SpellSlot, DamageSpell>> DamageLibrary { get; set; }

        public static float GetSpellDamage(this Obj_AI_Hero from, Obj_AI_Base target, SpellSlot slot,
            SpellStage stage = SpellStage.Default)
        {
            return 0f;
        }

        /// <summary>
        ///     Gets the automatic attack damage.
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        ///     from
        ///     or
        ///     to
        /// </exception>
        public static float GetAutoAttackDamage(this Obj_AI_Hero from, Obj_AI_Hero to)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from));
            }

            if (to == null)
            {
                throw new ArgumentNullException(nameof(to));
            }

            return CalculateDamage(from, to, DamageType.Physical, from.TotalAttackDamage);
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
        public static float CalculateMixedDamage(this Obj_AI_Base from, Obj_AI_Base target, float physicalDamage = 0f,
            float magicalDamage = 0f, float trueDamage = 0f)
        {
            return from.CalculateDamage(target, DamageType.Physical, physicalDamage) +
                   from.CalculateDamage(target, DamageType.Magical, magicalDamage) + trueDamage;
            ;
        }

        /// <summary>
        ///     Calculates the damage.
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <param name="damageType">Type of the damage.</param>
        /// <param name="damage">The damage.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="from" /> or <paramref name="to" /> was null.
        /// </exception>
        public static float CalculateDamage(this Obj_AI_Base from, Obj_AI_Base target, DamageType damageType,
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
                case DamageType.True:
                    return damage;
                case DamageType.Mixed:
                    throw new NotSupportedException(
                        "CalculateDamage does not support mixed damage. Instead, use Damage.CalculateMixedDamage");
                default:
                    throw new ArgumentOutOfRangeException(nameof(damageType), damageType, null);
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

            return
                Math.Max(
                    percentReceived * percentPassive * percentMod * (damage + flatPassive) + flatReceived +
                    otherDamageModifier, 0f);
        }
    }
}