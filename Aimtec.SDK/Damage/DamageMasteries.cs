
// ReSharper disable ConvertToLambdaExpression
// ReSharper disable LoopCanBeConvertedToQuery

namespace Aimtec.SDK.Damage
{
    using Aimtec.SDK.Extensions;

    using System;
    using System.Collections.Generic;

    internal class DamageMastery
    {
        static DamageMastery()
        {
            #region Masteries

            Masteries.Add(new Mastery
                              {
                                  Page = MasteryPage.Ferocity,
                                  Id = (uint)MasteryId.Ferocity.DoubleEdgedSword,
                                  DamageType = Mastery.MasteryDamageType.Percent,
                                  MasteryPercentDamage = (mastery, source, target) =>
                                      {
                                          return 1.03;
                                      }
                              });

            Masteries.Add(new Mastery
                              {
                                  Page = MasteryPage.Ferocity,
                                  Id = (uint)MasteryId.Ferocity.BattleTrance,
                                  DamageType = Mastery.MasteryDamageType.Percent,
                                  MasteryPercentDamage = (mastery, source, target) =>
                                      {
                                          var targetHero = target as Obj_AI_Hero;
                                          if (targetHero != null)
                                          {
                                              return 1.03;
                                          }

                                          return 1;
                                      }
                              });

            Masteries.Add(new Mastery
                              {
                                  Page = MasteryPage.Cunning,
                                  Id = (uint)MasteryId.Cunning.Assassin,
                                  DamageType = Mastery.MasteryDamageType.Percent,
                                  MasteryPercentDamage = (mastery, source, target) =>
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

            Masteries.Add(new Mastery
                              {
                                  Page = MasteryPage.Cunning,
                                  Id = (uint)MasteryId.Cunning.GreenFathersGift,
                                  DamageType = Mastery.MasteryDamageType.Magical,
                                  MasteryMagicalDamage = (mastery, source, target) =>
                                      {
                                          if (target != null)
                                          {
                                              return target.Health * 3 / 100;
                                          }

                                          return 0;
                                      }
                              });

            Masteries.Add(new Mastery
                              {
                                  Page = MasteryPage.Cunning,
                                  Id = (uint)MasteryId.Cunning.Merciless,
                                  DamageType = Mastery.MasteryDamageType.Percent,
                                  MasteryPercentDamage = (mastery, source, target) =>
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

            foreach (var mastery in Masteries)
            {
                if (!source.IsUsingMastery(mastery.Page, mastery.Id))
                {
                    continue;
                }

                var getMastery = source.GetMastery(mastery.Page, mastery.Id);
                switch (mastery.DamageType)
                {
                    case Mastery.MasteryDamageType.Physical:
                        totalPhysicalDamage += mastery.GetPhysicalDamage(getMastery, source, target);
                        break;

                    case Mastery.MasteryDamageType.Magical:
                        totalMagicalDamage += mastery.GetMagicalDamage(getMastery, source, target);
                        break;

                    case Mastery.MasteryDamageType.Percent:
                        totalPercentDamage *= mastery.GetPercentDamage(getMastery, source, target);
                        break;
                }
            }

            return new MasteryDamageResult(totalPhysicalDamage, totalMagicalDamage, totalPercentDamage);
        }

        public static List<Mastery> Masteries { get; set; } = new List<Mastery>();

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

        public class Mastery
        {
            public uint Id { get; set; }

            public MasteryPage Page { get; set; }

            public delegate double MasteryDamageDelegateHandler(Aimtec.Mastery mastery, Obj_AI_Hero source, AttackableUnit target);

            public MasteryDamageDelegateHandler MasteryPhysicalDamage { get; set; }
            public MasteryDamageDelegateHandler MasteryMagicalDamage { get; set; }
            public MasteryDamageDelegateHandler MasteryPercentDamage { get; set; }

            public double GetPhysicalDamage(Aimtec.Mastery mastery, Obj_AI_Hero source, AttackableUnit target)
            {
                if (this.MasteryPhysicalDamage != null)
                {
                    return this.MasteryPhysicalDamage(mastery, source, target);
                }

                return 0;
            }

            public double GetMagicalDamage(Aimtec.Mastery mastery, Obj_AI_Hero source, AttackableUnit target)
            {
                if (this.MasteryMagicalDamage != null)
                {
                    return this.MasteryMagicalDamage(mastery, source, target);
                }

                return 0;
            }

            public double GetPercentDamage(Aimtec.Mastery mastery, Obj_AI_Hero source, AttackableUnit target)
            {
                if (this.MasteryPercentDamage != null)
                {
                    return this.MasteryPercentDamage(mastery, source, target);
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
