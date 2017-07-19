
// ReSharper disable ConvertToLambdaExpression
// ReSharper disable LoopCanBeConvertedToQuery

namespace Aimtec.SDK.Damage
{
    using Aimtec.SDK.Extensions;

    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class DamageReduction
    {
        static DamageReduction()
        {
            #region Reductions

            Reductions.Add(new Reduction
                              {
                                  BuffName = "Exhaust",
                                  Type = Reduction.ReductionDamageType.Percent,
                                  ReductionPercentDamage = (source, attacker) =>
                                      {
                                           return 40;
                                      }
                              });

            Reductions.Add(new Reduction
                               {
                                   BuffName = "urgotentropypassive",
                                   Type = Reduction.ReductionDamageType.Percent,
                                   ReductionPercentDamage = (source, attacker) =>
                                       {
                                           return 15;
                                       }
                               });

            Reductions.Add(new Reduction
                               {
                                   BuffName = "urgotswapdef",
                                   Type = Reduction.ReductionDamageType.Percent,
                                   ReductionPercentDamage = (source, attacker) =>
                                       {
                                           return new[] { 30, 40, 50 }[source.SpellBook.GetSpell(SpellSlot.R).Level - 1];
                                       }
                               });

            Reductions.Add(new Reduction
                               {
                                   BuffName = "itemphantomdancerdebuff",
                                   Type = Reduction.ReductionDamageType.Percent,
                                   ReductionPercentDamage = (source, attacker) =>
                                       {
                                           var phantomdancerBuff = source.BuffManager.GetBuff("itemphantomdancerdebuff");
                                           if (phantomdancerBuff?.Caster.NetworkId == attacker?.NetworkId)
                                           {
                                                return 12;
                                           }

                                           return 0;
                                       }
                               });

            Reductions.Add(new Reduction
                               {
                                   BuffName = "BraumShieldRaise",
                                   Type = Reduction.ReductionDamageType.Percent,
                                   ReductionPercentDamage = (source, attacker) =>
                                       {
                                           return new[] { 30, 32.5, 35, 37.5, 40 }[source.SpellBook.GetSpell(SpellSlot.E).Level - 1];
                                       }
                               });

            Reductions.Add(new Reduction
                               {
                                   BuffName = "GalioW",
                                   Type = Reduction.ReductionDamageType.Percent,
                                   ReductionPercentDamage = (source, attacker) =>
                                       {
                                           return new[] { 20, 25, 30, 35, 40 }[source.SpellBook.GetSpell(SpellSlot.W).Level - 1] + 8 * (source.BonusSpellBlock / 100);
                                       }
                               });

            Reductions.Add(new Reduction
                               {
                                   BuffName = "GarenW",
                                   Type = Reduction.ReductionDamageType.Percent,
                                   ReductionPercentDamage = (source, attacker) =>
                                       {
										   if (ObjectManager.Get<GameObject>()
											       .Where(o => o.Type == GameObjectType.obj_GeneralParticleEmitter)
											       .Any(p => p.Name == "Garen_Base_W_Shoulder_L.troy" && p.IsAlly))
                                           {
                                                return 60;
                                           }

                                           return 30;
                                       }
                               });

            Reductions.Add(new Reduction
                               {
                                   BuffName = "GragasWSelf",
                                   Type = Reduction.ReductionDamageType.Percent,
                                   ReductionPercentDamage = (source, attacker) =>
                                       {
                                           return new[] { 10, 12, 14, 16, 18 }[source.SpellBook.GetSpell(SpellSlot.W).Level - 1];
                                       }
                               });

            Reductions.Add(new Reduction
                               {
                                   BuffName = "Meditate",
                                   Type = Reduction.ReductionDamageType.Percent,
                                   ReductionPercentDamage = (source, attacker) =>
                                       {
                                           return new[] { 50, 55, 60, 65, 70 }[source.SpellBook.GetSpell(SpellSlot.W).Level - 1] / (attacker is Obj_AI_Turret ? 2 : 1f);
                                       }
                               });

            #endregion
        }

        public static ReductionDamageResult ComputeReductionDamages(Obj_AI_Hero source, Obj_AI_Base attacker, DamageType damageType)
        {
            double flatDamageReduction = 0;
            double percentDamageReduction = 1;

            foreach (var reduction in Reductions)
            {
                if (source == null || !source.HasBuff(reduction.BuffName))
                {
                    continue;
                }

                switch (reduction.Type)
                {
                    case Reduction.ReductionDamageType.Flat:
                        flatDamageReduction += reduction.GetFlatDamage(source, attacker);
                        break;

                    case Reduction.ReductionDamageType.Percent:
                        percentDamageReduction *= 1 - reduction.GetPercentDamage(source, attacker) / 100;
                        break;
                }
            }

            return new ReductionDamageResult(flatDamageReduction, percentDamageReduction);
        }

        public static List<Reduction> Reductions { get; set; } = new List<Reduction>();

        public class ReductionDamageResult
        {
            public ReductionDamageResult(double flatDamageReduction, double percentDamageReduction)
            {
                this.FlatDamageReduction = flatDamageReduction;
                this.PercentDamageReduction = percentDamageReduction;
            }

            public double FlatDamageReduction { get; set; }
            public double PercentDamageReduction { get; set; }
        }

        public class Reduction
        {
            public string BuffName { get; set; }

            public delegate double ReductionDamageDelegateHandler(Obj_AI_Hero source, Obj_AI_Base attacker);

            public ReductionDamageDelegateHandler ReductionFlatDamage { get; set; }
            public ReductionDamageDelegateHandler ReductionPercentDamage { get; set; }

            public double GetFlatDamage(Obj_AI_Hero source, Obj_AI_Base attacker)
            {
                if (this.ReductionFlatDamage != null)
                {
                    return this.ReductionFlatDamage(source, attacker);
                }

                return 0;
            }

            public double GetPercentDamage(Obj_AI_Hero source, Obj_AI_Base attacker)
            {
                if (this.ReductionPercentDamage != null)
                {
                    return this.ReductionPercentDamage(source, attacker);
                }

                return 0;
            }

            public ReductionDamageType Type { get; set; }

            public enum ReductionDamageType
            {
                Flat,
                Percent
            }
        }
    }
}
