
// ReSharper disable ConvertToLambdaExpression
// ReSharper disable LoopCanBeConvertedToQuery

namespace Aimtec.SDK.Damage
{
    using Aimtec.SDK.Extensions;

    using System;
    using System.Collections.Generic;
    using System.Linq;

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
                                          return 1.03;
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
                                              return 1 - new[] { 0.6, 1.2, 1.8, 2.4, 3 }[mastery.Points - 1] / 100;
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

            var allMasteries = Masteries.Where(x => source.IsUsingMastery(x.Page, x.Id));
            var enumerable = allMasteries as IList<Mastery> ?? allMasteries.ToList();

            var physicalDamageMasteries = enumerable.Where(x => x.DamageType == Mastery.MasteryDamageType.Physical);
            var magicalDamageMasteries = enumerable.Where(x => x.DamageType == Mastery.MasteryDamageType.Magical);
            var percentDamageMasteries = enumerable.Where(x => x.DamageType == Mastery.MasteryDamageType.Percent);

            foreach (var mastery in physicalDamageMasteries)
            {
                totalPhysicalDamage += mastery.GetPhysicalDamage(source.GetMastery(mastery.Page, mastery.Id), source, target);
            }

            foreach (var mastery in magicalDamageMasteries)
            {
                totalMagicalDamage += mastery.GetMagicalDamage(source.GetMastery(mastery.Page, mastery.Id), source, target);
            }

            foreach (var mastery in percentDamageMasteries)
            {
                totalPercentDamage *= mastery.GetPercentDamage(source.GetMastery(mastery.Page, mastery.Id), source, target);
            }

            return new MasteryDamageResult(totalPhysicalDamage, totalMagicalDamage, totalPercentDamage);
        }

        public static List<Mastery> Masteries { get; set; } = new List<Mastery>();

        public class MasteryDamageResult
        {
            public MasteryDamageResult(double physicalDamage, double magicalDamage, double percentDamage)
            {
                this.PhysicalDamage = physicalDamage;
                this.MagicalDamage = magicalDamage;
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

            [Flags]
            public enum MasteryDamageType
            {
                Physical = 0x1,
                Magical = 0x2,
                Percent = 0x4
            }
        }
    }
}
