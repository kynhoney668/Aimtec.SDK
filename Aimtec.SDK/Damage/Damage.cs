namespace Aimtec.SDK.Damage
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Aimtec.SDK.Extensions;

    /// <summary>
    ///     Class Damage.
    /// </summary>
    public static class Damage
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Calculates the damage.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="damageType">Type of the damage.</param>
        /// <param name="amount">The amount.</param>
        /// <returns>System.Double.</returns>
        public static double CalculateDamage(
            this Obj_AI_Base source,
            Obj_AI_Base target,
            DamageType damageType,
            double amount)
        {
            var damage = 0d;
            switch (damageType)
            {
                case DamageType.Magical:
                    damage = source.CalculateMagicDamage(target, amount);
                    break;
                case DamageType.Physical:
                    damage = source.CalculatePhysicalDamage(target, amount);
                    break;
                case DamageType.Mixed:
                    damage = source.CalculateMixedDamage(target, damage / 2d, damage / 2d);
                    break;
                case DamageType.True:
                    damage = Math.Floor(source.GetPassiveDamage(target, Math.Max(amount, 0), DamageType.True));
                    break;
            }

            return damage;
        }

        /// <summary>
        ///     Calculates the mixed damage.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="physicalAmount">The physical amount.</param>
        /// <param name="magicalAmount">The magical amount.</param>
        /// <returns>System.Double.</returns>
        public static double CalculateMixedDamage(
            this Obj_AI_Base source,
            Obj_AI_Base target,
            double physicalAmount,
            double magicalAmount)
        {
            return source.CalculatePhysicalDamage(target, physicalAmount)
                + source.CalculateMagicDamage(target, magicalAmount);
        }

        /// <summary>
        ///     Gets the automatic attack damage.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <returns>System.Double.</returns>
        public static double GetAutoAttackDamage(this Obj_AI_Base source, Obj_AI_Base target)
        {
            var dmgPhysical = (double) source.TotalAttackDamage;
            var dmgMagical = 0d;
            var dmgPassive = 0d;
            var dmgReduce = 1d;

            var hero = source as Obj_AI_Hero;
            var targetHero = target as Obj_AI_Hero;

            var isMixDmg = false;

            if (hero != null)
            {
                // todo passives

                switch (hero.ChampionName)
                {
                    case "Kalista":
                        dmgPhysical *= 0.9;
                        break;
                    case "Jhin":
                        dmgPhysical += Math.Round(
                            (new[] { 2, 3, 4, 5, 6, 7, 8, 10, 12, 14, 16, 18, 20, 24, 28, 32, 36, 40 }[hero.Level - 1]
                                + Math.Round(hero.Crit * 100 / 10 * 4)
                                + Math.Round((hero.AttackSpeedMod - 1) * 100 / 10 * 2.5)) / 100 * dmgPhysical);
                        break;
                    case "Corki":
                        dmgPhysical /= 2;
                        dmgMagical = dmgPhysical;
                        isMixDmg = true;
                        break;
                    case "Quinn":
                        if (target.HasBuff("quinnw"))
                        {
                            dmgPhysical += 10 + 5 * hero.Level
                                + (0.14 + 0.02 * hero.Level) * hero.TotalAttackDamage;
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

                if (targetHero != null)
                {
                    // Dorans Shield
                    if (targetHero.HasItem(ItemId.DoransShield))
                    {
                        var subDmg = dmgPhysical + dmgMagical - 8;
                        dmgPhysical = !isMixDmg ? subDmg : (dmgMagical = subDmg / 2);
                    }
                }
                else if (target is Obj_AI_Minion)
                {
                    // RiftHerald P
                    if (!hero.IsMelee() && target.Team == GameObjectTeam.Neutral
                        && Regex.IsMatch(target.Name, "SRU_RiftHerald"))
                    {
                        dmgReduce *= 0.65;
                    }
                }
            }

            // Ninja Tabi
            if (targetHero != null && !(source is Obj_AI_Turret)
                && new[] { 3047, 1316, 1318, 1315, 1317 }.Any(i => targetHero.HasItem((uint) i)))
            {
                dmgReduce *= 0.9;
            }

            dmgPhysical = source.CalculatePhysicalDamage(target, dmgPhysical);
            dmgMagical = source.CalculateMagicDamage(target, dmgMagical);

            // Fizz P
            if (targetHero != null && targetHero.ChampionName == "Fizz")
            {
                dmgPhysical -= 4 + 2 * Math.Floor((targetHero.Level - 1) / 3d);
            }

            return Math.Max(
                Math.Floor((dmgPhysical + dmgMagical) * dmgReduce + source.GetPassiveFlatMod(target) + dmgPassive),
                0);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Calculates the magic damage.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="amount">The amount.</param>
        /// <returns>System.Double.</returns>
        private static double CalculateMagicDamage(this Obj_AI_Base source, Obj_AI_Base target, double amount)
        {
            if (amount <= 0)
            {
                return 0;
            }

            double value;

            if (target.SpellBlock < 0)
            {
                value = 2 - 100 / (100 - target.SpellBlock);
            }
            else if (target.SpellBlock * source.PercentMagicPenetration - source.FlatMagicPenetration < 0)
            {
                value = 1;
            }
            else
            {
                value = 100 / (100 + target.SpellBlock * source.PercentMagicPenetration - source.FlatMagicPenetration);
            }

            return Math.Max(
                Math.Floor(
                    source.GetDamageReduction(
                        target,
                        source.GetPassiveDamage(target, value, DamageType.Magical) * amount,
                        DamageType.Magical)),
                0);
        }

        /// <summary>
        ///     Calculates the physical damage.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="amount">The amount.</param>
        /// <returns>System.Double.</returns>
        private static double CalculatePhysicalDamage(this Obj_AI_Base source, Obj_AI_Base target, double amount)
        {
            if (amount <= 0)
            {
                return 0;
            }

            double armorPenetrationPercent = source.PercentArmorPenetration;
            double armorPenetrationFlat = source.FlatArmorPenetration;
            double bonusArmorPenetrationMod = source.PercentBonusArmorPenetration;

            // Minions return wrong percent values.
            if (source is Obj_AI_Minion)
            {
                armorPenetrationFlat = 0;
                armorPenetrationPercent = 1;
                bonusArmorPenetrationMod = 1;
            }

            // Turrets passive.
            if (source.Type == GameObjectType.obj_AI_Turret)
            {
                // todo turret passive damage
            }

            // Penetration can't reduce armor below 0.
            var armor = target.Armor;
            var bonusArmor = target.BonusArmor;

            double value;
            if (armor < 0)
            {
                value = 2 - 100 / (100 - armor);
            }
            else if (armor * armorPenetrationPercent - bonusArmor * (1 - bonusArmorPenetrationMod)
                - armorPenetrationFlat < 0)
            {
                value = 1;
            }
            else
            {
                value = 100 / (100 + armor * armorPenetrationPercent - bonusArmor * (1 - bonusArmorPenetrationMod)
                    - armorPenetrationFlat);
            }

            // Take into account the percent passives, flat passives and damage reduction.
            return Math.Max(
                Math.Floor(
                    source.GetDamageReduction(
                        target,
                        source.GetPassiveDamage(target, value, DamageType.Physical) * amount,
                        DamageType.Physical)),
                0);
        }

        /// <summary>
        ///     Gets the damage reduction.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="damageType">Type of the damage.</param>
        /// <returns>System.Double.</returns>
        private static double GetDamageReduction(
            this Obj_AI_Base source,
            Obj_AI_Base target,
            double amount,
            DamageType damageType)
        {
            var targetHero = target as Obj_AI_Hero;

            if (source is Obj_AI_Hero)
            {
                // Exhaust
                if (source.HasBuff("Exhaust"))
                {
                    amount *= 0.6;
                }

                // Urgot P
                if (source.HasBuff("urgotentropypassive"))
                {
                    amount *= 0.85;
                }

                if (targetHero != null)
                {
                    // Bond Of Stone
                    var bondofstoneBuffCount = targetHero.GetBuffCount("MasteryWardenOfTheDawn");
                    if (bondofstoneBuffCount > 0)
                    {
                        amount *= 1 - 0.06 * bondofstoneBuffCount;
                    }

                    // Phantom Dancer
                    var phantomdancerBuff = source.BuffManager.GetBuff("itemphantomdancerdebuff");
                    if (phantomdancerBuff != null && phantomdancerBuff.Caster.NetworkId == targetHero.NetworkId)
                    {
                        amount *= 0.88;
                    }
                }
            }

            if (targetHero != null)
            {
                // Alistar R
                if (targetHero.HasBuff("Ferocious Howl"))
                {
                    amount *= 0.3;
                }

                // Amumu E
                if (targetHero.HasBuff("Tantrum") && damageType == DamageType.Physical)
                {
                    amount -= new[] { 2, 4, 6, 8, 10 }[targetHero.SpellBook.GetSpell(SpellSlot.E).Level - 1];
                }

                // Braum E
                if (targetHero.HasBuff("BraumShieldRaise"))
                {
                    amount *= 1 - new[] { 0.3, 0.325, 0.35, 0.375, 0.4 }[targetHero
                        .SpellBook.GetSpell(SpellSlot.E).Level - 1];
                }

                // Galio R
                if (targetHero.HasBuff("GalioIdolOfDurand"))
                {
                    amount *= 0.5;
                }

                // Garen W
                if (targetHero.HasBuff("GarenW"))
                {
                    amount *= 0.7;
                }

                // Gragas W
                if (targetHero.HasBuff("GragasWSelf"))
                {
                    amount *= 1 - new[] { 0.1, 0.12, 0.14, 0.16, 0.18 }[targetHero.SpellBook.GetSpell(SpellSlot.W).Level
                        - 1];
                }

                // Kassadin P
                if (targetHero.HasBuff("VoidStone") && damageType == DamageType.Magical)
                {
                    amount *= 0.85;
                }

                // Katarina E
                if (targetHero.HasBuff("KatarinaEReduction"))
                {
                    amount *= 0.85;
                }

                // Maokai R
                if (targetHero.HasBuff("MaokaiDrainDefense") && !(source is Obj_AI_Turret))
                {
                    amount *= 0.8;
                }

                // MasterYi W
                if (targetHero.HasBuff("Meditate"))
                {
                    amount *= 1 - new[] { 0.5, 0.55, 0.6, 0.65, 0.7 }[targetHero.SpellBook.GetSpell(SpellSlot.W).Level
                        - 1] / (source is Obj_AI_Turret ? 2 : 1);
                }

                // Urgot R
                if (targetHero.HasBuff("urgotswapdef"))
                {
                    amount *= 1 - new[] { 0.3, 0.4, 0.5 }[targetHero.SpellBook.GetSpell(SpellSlot.R).Level - 1];
                }

                // Yorick P
                if (targetHero.HasBuff("YorickUnholySymbiosis"))
                {
                    amount *= 1 - ObjectManager
                        .Get<Obj_AI_Minion>().Count(
                            g => g.Team == targetHero.Team && (g.Name.Equals("Clyde") || g.Name.Equals("Inky")
                                || g.Name.Equals("Blinky")
                                || g.HasBuff("yorickunholysymbiosis")
                                && g.BuffManager.GetBuff("yorickunholysymbiosis").Caster.NetworkId
                                == targetHero.NetworkId)) * 0.05;
                }
            }

            return amount;
        }

        /// <summary>
        ///     Gets the passive damage.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="damageType">Type of the damage.</param>
        /// <returns>System.Double.</returns>
        private static double GetPassiveDamage(
            this Obj_AI_Base source,
            Obj_AI_Base target,
            double amount,
            DamageType damageType)
        {
            if (source is Obj_AI_Turret)
            {
                var minion = target as Obj_AI_Minion;

                if (minion != null)
                {
                    if (minion.Name.Contains("Siege"))
                    {
                        amount *= 0.7;
                    }
                    else if (minion.Name.Contains("MinionMelee") || minion.Name.Contains("MinionRanged"))
                    {
                        amount *= 1.14285714285714;
                    }
                }
            }

            var hero = source as Obj_AI_Hero;

            if (hero != null)
            {
                // todo masteries
            }
            return amount;
        }

        /// <summary>
        ///     Gets the passive flat mod.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <returns>System.Double.</returns>
        private static double GetPassiveFlatMod(this GameObject source, Obj_AI_Base target)
        {
            // todo masteries
            return 0d;
        }

        #endregion
    }
}