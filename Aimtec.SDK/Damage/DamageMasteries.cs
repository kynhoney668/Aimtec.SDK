
// ReSharper disable ConvertToLambdaExpression
// ReSharper disable LoopCanBeConvertedToQuery

namespace Aimtec.SDK.Damage
{
    using Aimtec.SDK.Extensions;

    using System;
    using System.Collections.Generic;

    internal class DamageMasteries
    {
        static DamageMasteries()
        {
            #region Masteries

            Masteries.Add(new DamageMastery
                              {
                                  Page = MasteryPage.Ferocity,
                                  Id = (uint)MasteryId.Ferocity.DoubleEdgedSword,
                                  DamageType = DamageMastery.MasteryDamageType.Percent,
                                  MasteryDamage = (mastery, source, target) =>
                                      {
                                          return 1.03;
                                      }
                              });

            Masteries.Add(new DamageMastery
                              {
                                  Page = MasteryPage.Ferocity,
                                  Id = (uint)MasteryId.Ferocity.BattleTrance,
                                  DamageType = DamageMastery.MasteryDamageType.Percent,
                                  MasteryDamage = (mastery, source, target) =>
                                      {
                                          var targetHero = target as Obj_AI_Hero;
                                          if (targetHero != null)
                                          {
                                              return 1.03;
                                          }

                                          return 1;
                                      }
                              });

            Masteries.Add(new DamageMastery
                              {
                                  Page = MasteryPage.Cunning,
                                  Id = (uint)MasteryId.Cunning.Assassin,
                                  DamageType = DamageMastery.MasteryDamageType.Percent,
                                  MasteryDamage = (mastery, source, target) =>
                                      {
                                          var targetHero = target as Obj_AI_Hero;
                                          if (targetHero != null &&
                                              source.CountAllyHeroesInRange(800f) == 0)
                                          {
                                              return 1.02;
                                          }

                                          return 1;
                                      }
                              });

            Masteries.Add(new DamageMastery
                              {
                                  Page = MasteryPage.Cunning,
                                  Id = (uint)MasteryId.Cunning.GreenFathersGift,
                                  DamageType = DamageMastery.MasteryDamageType.Magical,
                                  MasteryDamage = (mastery, source, target) =>
                                      {
                                          if (target != null)
                                          {
                                              return target.Health * 3 / 100;
                                          }

                                          return 0;
                                      }
                              });

            Masteries.Add(new DamageMastery
                              {
                                  Page = MasteryPage.Cunning,
                                  Id = (uint)MasteryId.Cunning.Merciless,
                                  DamageType = DamageMastery.MasteryDamageType.Percent,
                                  MasteryDamage = (mastery, source, target) =>
                                      {
                                          var targetHero = target as Obj_AI_Hero;
                                          if (targetHero?.HealthPercent() < 40)
                                          {
                                              return 1 + new[] { 0.6, 1.2, 1.8, 2.4, 3 }[mastery.Points - 1] / 100;
                                          }

                                          return 1;
                                      }
                              });

            #endregion
        }

        public static MasteryDamageResult ComputeMasteryDamages(Obj_AI_Hero source, AttackableUnit target)
        {
            double totalPhysicalDamage = 0;
            double totalMagicalDamage = 0;
            double totalPercentDamage = 1;

            foreach (var damageMastery in Masteries)
            {
                var mastery = source.GetMastery(damageMastery.Page, damageMastery.Id);

                if (mastery == null)
                {
                    continue;
                }

                switch (damageMastery.DamageType)
                {
                    case DamageMastery.MasteryDamageType.Physical:
                        totalPhysicalDamage += damageMastery.GetMasteryDamage(mastery, source, target);
                        break;

                    case DamageMastery.MasteryDamageType.Magical:
                        totalMagicalDamage += damageMastery.GetMasteryDamage(mastery, source, target);
                        break;

                    case DamageMastery.MasteryDamageType.Percent:
                        totalPercentDamage *= damageMastery.GetMasteryDamage(mastery, source, target);
                        break;
                }
            }

            return new MasteryDamageResult(totalPhysicalDamage, totalMagicalDamage, totalPercentDamage);
        }

        public static List<DamageMastery> Masteries { get; set; } = new List<DamageMastery>();

        public class MasteryDamageResult
        {
            public MasteryDamageResult(double physicalDamage, double magicalDamage, double percentDamage)
            {
                this.PhysicalDamage = Math.Floor(physicalDamage);
                this.MagicalDamage = Math.Floor(magicalDamage);
                this.PercentDamage = percentDamage;
            }

            public double PhysicalDamage { get; set; }
            public double MagicalDamage { get; set; }
            public double PercentDamage { get; set; }
        }

        public class DamageMastery
        {
            public uint Id { get; set; }

            public MasteryPage Page { get; set; }

            public delegate double MasteryDamageDelegateHandler(Mastery mastery, Obj_AI_Hero source, AttackableUnit target);

            public MasteryDamageDelegateHandler MasteryDamage { get; set; }

            public double GetMasteryDamage(Mastery mastery, Obj_AI_Hero source, AttackableUnit target)
            {
                if (this.MasteryDamage != null)
                {
                    return this.MasteryDamage(mastery, source, target);
                }

                return 0;
            }
 
            public MasteryDamageType DamageType { get; set; }

            public enum MasteryDamageType
            {
                Physical,
                Magical,
                Percent
            }
        }
    }
}
