namespace Aimtec.SDK.Damage
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Aimtec.SDK.Damage.JSON;
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
        /// <exception cref="ArgumentOutOfRangeException">damageType - null</exception>
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
                default: throw new ArgumentOutOfRangeException(nameof(damageType), damageType, null);
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
            //var dmgPassive = 0d;
            var dmgReduce = 1d;

            var hero = source as Obj_AI_Hero;
            var targetHero = target as Obj_AI_Hero;

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
                        break;
                    case "Quinn":
                        if (target.HasBuff("quinnw"))
                        {
                            dmgPhysical += 10 + 5 * hero.Level + (0.14 + 0.02 * hero.Level) * hero.TotalAttackDamage;
                        }
                        break;
                    case "Galio":
                        if (hero.HasBuff("galiopassivebuff"))
                        {
                            dmgMagical += 8 + 4 * hero.Level + hero.TotalAttackDamage + 0.6 * hero.TotalAbilityDamage + 0.6 * hero.BonusSpellBlock;
                        }
                        break;
                    // TODO getting the actual buffname
                    case "Sejuani":
                        if (target.HasBuff("SejuaniEMarker")) //SejuaniEMarkerMax
                        {
                            switch (target.Type)
                            {
                                case obj_AI_Hero:
                                    if (hero.Level < 7)
                                    {
                                        dmgMagical += 0.1 * targetHero.MaxHealth;
                                    }
                                    else if (hero.Level < 14)
                                    {
                                        dmgMagical += 0.15 * targetHero.MaxHealth;
                                    }
                                    else
                                    {
                                        dmgMagical += 0.2 * targetHero.MaxHealth;
                                    }
                                    break;
                                case obj_AI_Minion:
                                    dmgMagical += 400;
                                    break;
                            }
                        }
                        break;
                }

                if (target is Obj_AI_Minion)
                {
                    // RiftHerald P
                    if (!hero.IsMelee() && target.Team == GameObjectTeam.Neutral
                        && Regex.IsMatch(target.Name, "SRU_RiftHerald"))
                    {
                        dmgReduce *= 0.65;
                    }

                    // Dorans Shield
                    if (hero.HasItem(ItemId.DoransShield))
                    {
                        dmgPhysical += 5;
                    }
                }
            }

            // Ninja Tabi
            if (targetHero != null && !(source is Obj_AI_Turret) && targetHero.HasItem(3047))
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

            return Math.Max(Math.Floor((dmgPhysical + dmgMagical) * dmgReduce + source.GetPassiveFlatMod(target)), 0);
        }

        /// <summary>
        ///     Gets the spell damage.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="spellSlot">The spell slot.</param>
        /// <param name="stage">The stage.</param>
        /// <returns>System.Double.</returns>
        public static double GetSpellDamage(
            this Obj_AI_Hero source,
            Obj_AI_Base target,
            SpellSlot spellSlot,
            DamageStage stage = DamageStage.Default)
        {
            if (source == null || !source.IsValid || target == null || !target.IsValid)
            {
                return 0;
            }

            if (!DamageLibrary.Damages.TryGetValue(source.ChampionName, out ChampionDamage value))
            {
                return 0;
            }

            var spellData = value.GetSlot(spellSlot)?.FirstOrDefault(e => e.Stage == stage)?.SpellData;
            if (spellData == null)
            {
                return 0;
            }

            var spellLevel = source.SpellBook.GetSpell(
                                       spellData.ScaleSlot != SpellSlot.Unknown ? spellData.ScaleSlot : spellSlot)
                                   .Level;

            if (spellLevel == 0)
            {
                return 0;
            }

            var alreadyAdd1 = false;
            var alreadyAdd2 = false;

            var targetHero = target as Obj_AI_Hero;
            var targetMinion = target as Obj_AI_Minion;

            var dmgBase = 0d;
            var dmgBonus = 0d;
            var dmgPassive = 0d;
            var dmgReduce = 1d;

            if (spellData.DamagesPerLvl?.Count > 0)
            {
                dmgBase = spellData.DamagesPerLvl[Math.Min(source.Level - 1, spellData.DamagesPerLvl.Count - 1)];
            }
            else if (spellData.Damages?.Count > 0)
            {
                dmgBase = spellData.Damages[Math.Min(spellLevel - 1, spellData.Damages.Count - 1)];

                if (!string.IsNullOrEmpty(spellData.ScalingBuff))
                {
                    var buffCount = (spellData.ScalingBuffTarget == DamageScalingTarget.Source ? source : target)
                        .GetBuffCount(spellData.ScalingBuff);

                    dmgBase = buffCount > 0 ? dmgBase * (buffCount + spellData.ScalingBuffOffset) : 0;
                }
            }
            if (dmgBase > 0)
            {
                if (targetMinion != null && spellData.BonusDamageOnMinion?.Count > 0)
                {
                    dmgBase += spellData.BonusDamageOnMinion[Math.Min(
                        spellLevel - 1,
                        spellData.BonusDamageOnMinion.Count - 1)];
                }

                if (spellData.IsApplyOnHit || spellData.IsModifiedDamage
                    || spellData.SpellEffectType == SpellEffectType.Single)
                {
                    alreadyAdd1 = true;
                }

                dmgBase = source.CalculateDamage(target, spellData.DamageType, dmgBase);

                if (spellData.IsModifiedDamage && spellData.DamageType == DamageType.Physical && targetHero != null
                    && targetHero.ChampionName == "Fizz")
                {
                    dmgBase -= 4 + (2 * Math.Floor((targetHero.Level - 1) / 3d));
                    alreadyAdd2 = true;
                }
            }
            if (spellData.BonusDamages?.Count > 0)
            {
                foreach (var bonusDmg in spellData.BonusDamages)
                {
                    var dmg = source.GetBonusSpellDamage(target, bonusDmg, spellLevel - 1);

                    if (dmg <= 0)
                    {
                        continue;
                    }

                    if (!alreadyAdd1 && (spellData.IsModifiedDamage
                        || spellData.SpellEffectType == SpellEffectType.Single))
                    {
                        alreadyAdd1 = true;
                    }

                    dmgBonus += source.CalculateDamage(target, bonusDmg.DamageType, dmg);

                    if (alreadyAdd2 || !spellData.IsModifiedDamage || bonusDmg.DamageType != DamageType.Physical
                        || targetHero == null || targetHero.ChampionName != "Fizz")
                    {
                        continue;
                    }

                    dmgBonus -= 4 + (2 * Math.Floor((targetHero.Level - 1) / 3d));
                    alreadyAdd2 = true;
                }
            }

            var totalDamage = dmgBase + dmgBonus;

            if (totalDamage > 0)
            {
                if (spellData.ScalePerTargetMissHealth > 0)
                {
                    totalDamage *= (target.MaxHealth - target.Health) / target.MaxHealth
                        * spellData.ScalePerTargetMissHealth + 1;
                }

                if (target is Obj_AI_Minion && spellData.MaxDamageOnMinion?.Count > 0)
                {
                    totalDamage = Math.Min(
                        totalDamage,
                        spellData.MaxDamageOnMinion[Math.Min(spellLevel - 1, spellData.MaxDamageOnMinion.Count - 1)]);
                }

                if (spellData.IsApplyOnHit || spellData.IsModifiedDamage)
                {
                    dmgPassive += 0; // todo get passive shit

                    if (targetHero != null)
                    {
                        if (spellData.IsModifiedDamage && targetHero.HasItem(3047))
                        {
                            dmgReduce *= 0.9;
                        }
                    }
                }
            }

            return Math.Max(
                Math.Floor(
                    totalDamage * dmgReduce + (spellData.IsApplyOnHit || spellData.IsModifiedDamage
                        ? source.GetPassiveFlatMod(target)
                        : 0) + dmgPassive),
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
                if (targetHero.HasBuff("FerociousHowl"))
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

                // Galio W TODO
                if (targetHero.HasBuff("galioSOMEONEPLSHELPMEWITHBUFFNAMESTY"))
                {
                    amount *= 1 - new[] { 0.2, 0.25, 0.3, 0.35, 0.4 }[targetHero
                        .SpellBook.GetSpell(SpellSlot.W).Level - 1] + 0.08 * (int)(targetHero.BonusSpellBlock/100);
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
