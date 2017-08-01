
// ReSharper disable ConvertToLambdaExpression
// ReSharper disable LoopCanBeConvertedToQuery

namespace Aimtec.SDK.Damage
{
    using Aimtec.SDK.Extensions;
    
    using System.Collections.Generic;
    using System.Linq;

    internal class DamageReductions
    {
        static DamageReductions()
        {
            #region Reductions

            Reductions.Add(new DamageReduction
            {
                                  BuffName = "Exhaust",
                                  Type = DamageReduction.ReductionDamageType.Percent,
                                  ReductionDamage = (source, attacker) =>
                                      {
                                           return 40;
                                      }
                              });

            Reductions.Add(new DamageReduction
            {
                                   BuffName = "itemphantomdancerdebuff",
                                   Type = DamageReduction.ReductionDamageType.Percent,
                                   ReductionDamage = (source, attacker) =>
                                       {
                                           var phantomdancerBuff = source.BuffManager.GetBuff("itemphantomdancerdebuff");
                                           if (phantomdancerBuff?.Caster.NetworkId == attacker?.NetworkId)
                                           {
                                                return 12;
                                           }

                                           return 0;
                                       }
                               });

            Reductions.Add(new DamageReduction
            {
                                   BuffName = "BraumShieldRaise",
                                   Type = DamageReduction.ReductionDamageType.Percent,
                                   ReductionDamage = (source, attacker) =>
                                       {
                                           return new[] { 30, 32.5, 35, 37.5, 40 }[source.SpellBook.GetSpell(SpellSlot.E).Level - 1];
                                       }
                               });

            Reductions.Add(new DamageReduction
            {
                                   BuffName = "GalioW",
                                   Type = DamageReduction.ReductionDamageType.Percent,
                                   ReductionDamage = (source, attacker) =>
                                       {
                                           return new[] { 20, 25, 30, 35, 40 }[source.SpellBook.GetSpell(SpellSlot.W).Level - 1] + 8 * (source.BonusSpellBlock / 100);
                                       }
                               });

            Reductions.Add(new DamageReduction
            {
                                   BuffName = "GarenW",
                                   Type = DamageReduction.ReductionDamageType.Percent,
                                   ReductionDamage = (source, attacker) =>
                                       {
										   if (ObjectManager.Get<GameObject>().Any(p => p.IsAlly && p.Name == "Garen_Base_W_Shoulder_L.troy"))
                                           {
                                                return 60;
                                           }

                                           return 30;
                                       }
                               });

            Reductions.Add(new DamageReduction
            {
                                   BuffName = "GragasWSelf",
                                   Type = DamageReduction.ReductionDamageType.Percent,
                                   ReductionDamage = (source, attacker) =>
                                       {
                                           return new[] { 10, 12, 14, 16, 18 }[source.SpellBook.GetSpell(SpellSlot.W).Level - 1];
                                       }
                               });

            Reductions.Add(new DamageReduction
                               {
                                   BuffName = "Meditate",
                                   Type = DamageReduction.ReductionDamageType.Percent,
                                   ReductionDamage = (source, attacker) =>
                                       {
                                           return new[] { 50, 55, 60, 65, 70 }[source.SpellBook.GetSpell(SpellSlot.W).Level - 1] / (attacker is Obj_AI_Turret ? 2 : 1f);
                                       }
                               });

            #endregion
        }

        public static ReductionDamageResult ComputeReductions(Obj_AI_Hero source, Obj_AI_Base attacker, DamageType damageType)
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
                    case DamageReduction.ReductionDamageType.Flat:
                        flatDamageReduction += reduction.GetDamageReduction(source, attacker);
                        break;

                    case DamageReduction.ReductionDamageType.Percent:
                        percentDamageReduction *= 1 - reduction.GetDamageReduction(source, attacker) / 100;
                        break;
                }
            }

            return new ReductionDamageResult(flatDamageReduction, percentDamageReduction);
        }

        public static List<DamageReduction> Reductions { get; set; } = new List<DamageReduction>();

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

        public class DamageReduction
        {
            public string BuffName { get; set; }

            public delegate double ReductionDamageDelegateHandler(Obj_AI_Hero source, Obj_AI_Base attacker);

            public ReductionDamageDelegateHandler ReductionDamage { get; set; }

            public double GetDamageReduction(Obj_AI_Hero source, Obj_AI_Base attacker)
            {
                if (this.ReductionDamage != null)
                {
                    return this.ReductionDamage(source, attacker);
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
