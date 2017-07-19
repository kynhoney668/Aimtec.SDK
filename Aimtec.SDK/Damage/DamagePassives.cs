
// ReSharper disable ConvertToLambdaExpression
// ReSharper disable LoopCanBeConvertedToQuery

namespace Aimtec.SDK.Damage
{
    using Aimtec.SDK.Extensions;

    using System;
    using System.Collections.Generic;

    using Aimtec.SDK.Damage.JSON;

    internal class DamagePassives
    {
        static DamagePassives()
        {
            #region Physical Damage Passives

            Passives.Add(new DamagePassive
                          {
                              Name = "Aatrox",
                              DamageType = DamagePassive.PassiveDamageType.FlatPhysical,
                              PassiveDamage = (source, target) =>
                                  {
                                      if (source.HasBuff("aatroxwpower") &&
                                          source.HasBuff("aatroxwonhpowerbuff"))
                                      {
                                          return source.GetSpellDamage(target, SpellSlot.W);
                                      }

                                      return 0;
                                  }
                          });

            Passives.Add(new DamagePassive
                             {
                                 Name = "Akali",
                                 DamageType = DamagePassive.PassiveDamageType.FlatMagical,
                                 PassiveDamage = (source, target) =>
                                     {
                                         if (target.HasBuff("AkaliMota"))
                                         {
                                             return source.GetSpellDamage(target, SpellSlot.Q, DamageStage.Detonation);
                                         }

                                         if (source.HasBuff("akalishadowstate"))
                                         {
                                             return new[] { 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 40, 50, 60, 70, 80, 90, 100 }[source.Level - 1];
                                         }

                                         return 0;
                                     }
                             });

            Passives.Add(new DamagePassive
                             {
                                 Name = "Alistar",
                                 DamageType = DamagePassive.PassiveDamageType.FlatMagical,
                                 PassiveDamage = (source, target) =>
                                     {
                                         if (target.HasBuff("alistartrample"))
                                         {
                                             return 40 + 15 * source.Level;
                                         }

                                         return 0;
                                     }
                             });

            Passives.Add(new DamagePassive
                             {
                                 Name = "Ashe",
                                 DamageType = DamagePassive.PassiveDamageType.PercentPhysical,
                                 PassiveDamage = (source, target) =>
                                     {
                                         if (source.HasBuff("asheqbuff"))
                                         {
                                             return source.GetSpellDamage(target, SpellSlot.Q);
                                         }

                                         return 0;
                                     }
                             });

            Passives.Add(new DamagePassive
                             {
                                 Name = "Ashe",
                                 DamageType = DamagePassive.PassiveDamageType.FlatPhysical,
                                 PassiveDamage = (source, target) =>
                                     {
                                         if (target.HasBuff("ashepassiveslow"))
                                         {
                                             return (10 + source.Crit*100 * (1 + (source.HasItem(ItemId.InfinityEdge) ? 0.5 : 0))) * source.TotalAttackDamage;
                                         }

                                         return 0;
                                     }
                             });

            Passives.Add(new DamagePassive
                             {
                                 Name = "Bard",
                                 DamageType = DamagePassive.PassiveDamageType.FlatMagical,
                                 PassiveDamage = (source, target) =>
                                     {
                                         if (source.GetBuffCount("bardpspiritammocount") > 0)
                                         {
                                             return 30 + 15 * (source.GetBuffCount("bardpdisplaychimecount") / 5) + 0.3 * source.TotalAbilityDamage;
                                         }

                                         return 0;
                                     }
                             });

            Passives.Add(new DamagePassive
                             {
                                 Name = "Blitzcrank",
                                 DamageType = DamagePassive.PassiveDamageType.FlatPhysical,
                                 PassiveDamage = (source, target) =>
                                     {
                                         if (source.HasBuff("powerfist"))
                                         {
                                             return source.TotalAttackDamage;
                                         }

                                         return 0;
                                     }
                             });

            Passives.Add(new DamagePassive
                             {
                                 Name = "Braum",
                                 DamageType = DamagePassive.PassiveDamageType.FlatMagical,
                                 PassiveDamage = (source, target) =>
                                     {
                                         if (target.HasBuff("braummarkstunreduction"))
                                         {
                                             return source.GetSpellDamage(target, SpellSlot.Q, DamageStage.Buff);
                                         }

                                         return 0;
                                     }
                             });

            Passives.Add(new DamagePassive
                             {
                                 Name = "Caitlyn",
                                 DamageType = DamagePassive.PassiveDamageType.FlatPhysical,
                                 PassiveDamage = (source, target) =>
                                     {
                                         if (source.HasBuff("caitlynheadshot") ||
                                             source.HasBuff("caitlynheadshotrangecheck") && target.HasBuff("caitlynyordletrapinternal"))
                                         {
                                             var damage = 0d;
                                             switch (target.Type)
                                             {
                                                 case GameObjectType.obj_AI_Minion:
                                                     damage = 1.5 * source.TotalAttackDamage;
                                                     break;

                                                 case GameObjectType.obj_AI_Hero:
                                                     var critDamageMultiplier = source.HasItem(ItemId.InfinityEdge) ? 2.5 : 2;
                                                     damage = (50 + 0.5 * critDamageMultiplier * source.Crit*100) / 100 * source.TotalAttackDamage;
                                                     if (target.HasBuff("caitlynyordletrapinternal"))
                                                     {
                                                         damage +=
                                                            new[] { 30, 70, 110, 150, 190 }[source.SpellBook.GetSpell(SpellSlot.W).Level - 1] + 0.7 * source.TotalAttackDamage;
                                                     }
                                                     break;
                                             }

                                             return damage;
                                         }

                                         return 0;
                                     }
                             });

            Passives.Add(new DamagePassive
                             {
                                 Name = "Camille",
                                 DamageType = DamagePassive.PassiveDamageType.FlatMagical,
                                 PassiveDamage = (source, target) =>
                                     {
                                         if (source.HasBuff("camiller"))
                                         {
                                             var level = source.SpellBook.GetSpell(SpellSlot.R).Level - 1;
                                             return new[] { 5, 10, 15 }[level] + new[] { 0.04, 0.06, 0.08 }[level] * (target.MaxHealth - target.Health);
                                         }

                                         return 0;
                                     }
                             });

            Passives.Add(new DamagePassive
                             {
                                 Name = "ChoGath",
                                 DamageType = DamagePassive.PassiveDamageType.FlatMagical,
                                 PassiveDamage = (source, target) =>
                                     {
                                         if (source.HasBuff("VorpalSpikes"))
                                         {
                                             return source.GetSpellDamage(target, SpellSlot.E);
                                         }

                                         return 0;
                                     }
                             });

            Passives.Add(new DamagePassive
                             {
                                 Name = "Corki",
                                 DamageType = DamagePassive.PassiveDamageType.FlatMagical,
                                 PassiveDamage = (source, target) =>
                                     {
                                         return 0.8 * source.TotalAttackDamage;
                                     }
                             });

            Passives.Add(new DamagePassive
                             {
                                 Name = "Corki",
                                 DamageType = DamagePassive.PassiveDamageType.PercentPhysical,
                                 PassiveDamage = (source, target) =>
                                     {
                                         return 0.2;
                                     }
                             });

            Passives.Add(new DamagePassive
                             {
                                 Name = "Darius",
                                 DamageType = DamagePassive.PassiveDamageType.PercentPhysical,
                                 PassiveDamage = (source, target) =>
                                     {
                                         if (source.HasBuff("dariusnoxiantacticsonh"))
                                         {
                                             return source.GetSpellDamage(target, SpellSlot.W);
                                         }

                                         return 0;
                                     }
                             });

            Passives.Add(new DamagePassive
                             {
                                 Name = "Diana",
                                 DamageType = DamagePassive.PassiveDamageType.FlatMagical,
                                 PassiveDamage = (source, target) =>
                                     {
                                         if (source.GetBuffCount("dianapassivemarker") == 2)
                                         {
                                             return new[] { 20, 25, 30, 35, 40, 50, 60, 70, 80, 90, 105, 120, 135, 155, 175, 200, 225, 250 }[source.Level - 1] + 0.8 * source.TotalAbilityDamage;
                                         }

                                         return 0;
                                     }
                             });

            Passives.Add(new DamagePassive
                             {
                                 Name = "Draven",
                                 DamageType = DamagePassive.PassiveDamageType.FlatPhysical,
                                 PassiveDamage = (source, target) =>
                                     {
                                         if (source.HasBuff("dravenspinningattack"))
                                         {
                                             return source.GetSpellDamage(target, SpellSlot.Q);
                                         }

                                         return 0;
                                     }
                             });

            Passives.Add(new DamagePassive
                             {
                                 Name = "Ekko",
                                 DamageType = DamagePassive.PassiveDamageType.FlatMagical,
                                 PassiveDamage = (source, target) =>
                                     {
                                         if (target.GetBuffCount("ekkostacks") == 2)
                                         {
                                             return new[] { 30, 40, 50, 60, 70, 80, 85, 90, 95, 100, 105, 110, 115, 120, 125, 130, 135, 140 }[source.Level - 1] + 0.8 * source.TotalAbilityDamage;
                                         }

                                         return 0;
                                     }
                             });

            Passives.Add(new DamagePassive
                             {
                                 Name = "Galio",
                                 DamageType = DamagePassive.PassiveDamageType.FlatMagical,
                                 PassiveDamage = (source, target) =>
                                     {
                                         if (source.HasBuff("galiopassivebuff"))
                                         {
                                             return 8 + 4 * source.Level + source.TotalAttackDamage + (source.TotalAbilityDamage + 0.4) + source.BonusSpellBlock * 0.4;
                                         }

                                         return 0;
                                     }
                             });

            Passives.Add(new DamagePassive
                             {
                                 Name = "Garen",
                                 DamageType = DamagePassive.PassiveDamageType.FlatPhysical,
                                 PassiveDamage = (source, target) =>
                                     {
                                         if (target.HasBuff("garenq"))
                                         {
                                             return source.GetSpellDamage(target, SpellSlot.Q);
                                         }

                                         return 0;
                                     }
                             });

            Passives.Add(new DamagePassive
                             {
                                 Name = "Gnar",
                                 DamageType = DamagePassive.PassiveDamageType.FlatMagical,
                                 PassiveDamage = (source, target) =>
                                     {
                                         if (target.GetBuffCount("gnarwproc") == 2)
                                         {
                                             return source.GetSpellDamage(target, SpellSlot.W);
                                         }

                                         return 0;
                                     }
                             });

            Passives.Add(new DamagePassive
                             {
                                 Name = "Gragas",
                                 DamageType = DamagePassive.PassiveDamageType.FlatMagical,
                                 PassiveDamage = (source, target) =>
                                     {
                                         if (target.HasBuff("gragaswattackbuff"))
                                         {
                                             return source.GetSpellDamage(target, SpellSlot.W);
                                         }

                                         return 0;
                                     }
                             });

            Passives.Add(new DamagePassive
                             {
                                 Name = "Hecarim",
                                 DamageType = DamagePassive.PassiveDamageType.FlatPhysical,
                                 PassiveDamage = (source, target) =>
                                     {
                                         if (target.HasBuff("hecarimrampspeed"))
                                         {
                                             return source.GetSpellDamage(target, SpellSlot.E);
                                         }

                                         return 0;
                                     }
                             });

            Passives.Add(new DamagePassive
                             {
                                 Name = "Kalista",
                                 DamageType = DamagePassive.PassiveDamageType.PercentPhysical,
                                 PassiveDamage = (source, target) =>
                                     {
                                         return 0.9;
                                     }
                             });

            Passives.Add(new DamagePassive
                             {
                                 Name = "Quinn",
                                 DamageType = DamagePassive.PassiveDamageType.FlatPhysical,
                                 PassiveDamage = (source, target) =>
                                     {
                                         if (target.HasBuff("quinnw"))
                                         {
                                             return 10 + 5 * source.Level + (0.14 + 0.02 * source.Level) * source.TotalAttackDamage;
                                         }

                                         return 0;
                                     }
                             });

            Passives.Add(new DamagePassive
                             {
                                 Name = "Sejuani",
                                 DamageType = DamagePassive.PassiveDamageType.FlatMagical,
                                 PassiveDamage = (source, target) =>
                                     {
                                         if (target.HasBuff("sejuanistun"))
                                         {
                                             switch (target.Type)
                                             {
                                                 case GameObjectType.obj_AI_Hero:
                                                     if (source.Level < 7)
                                                     {
                                                         return 0.1 * target.MaxHealth;
                                                     }
                                                     else if (source.Level < 14)
                                                     {
                                                         return 0.15 * target.MaxHealth;
                                                     }
                                                     else
                                                     {
                                                         return 0.2 * target.MaxHealth;
                                                     }

                                                 case GameObjectType.obj_AI_Minion:
                                                     return 400;
                                             }
                                         }

                                         return 0;
                                     }
                             });

            #endregion
        }

        public static PassiveDamageResult ComputePassiveDamages(Obj_AI_Hero source, Obj_AI_Base target)
        {
            double totalPhysicalDamage = 0;
            double totalMagicalDamage = 0;
            double totalTrueDamage = 0;

            double totalPhysicalDamageMod = 1;
            double totalMagicalDamageMod = 1;
            double totalTrueDamageMod = 1;

            foreach (var passive in Passives)
            {
                if (source.ChampionName != passive.Name)
                {
                    continue;
                }

                var passiveDamage = passive.GetDamage(source, target);

                switch (passive.DamageType)
                {
                    case DamagePassive.PassiveDamageType.FlatPhysical:
                        totalPhysicalDamage += passiveDamage;
                        break;

                    case DamagePassive.PassiveDamageType.FlatMagical:
                        totalMagicalDamage += passiveDamage;
                        break;

                    case DamagePassive.PassiveDamageType.FlatTrue:
                        totalTrueDamage += passiveDamage;
                        break;

                    case DamagePassive.PassiveDamageType.PercentPhysical:
                        totalPhysicalDamageMod *= passiveDamage;
                        break;

                    case DamagePassive.PassiveDamageType.PercentMagical:
                        totalMagicalDamageMod *= passiveDamage;
                        break;

                    case DamagePassive.PassiveDamageType.PercentTrue:
                        totalTrueDamageMod *= passiveDamage;
                        break;
                }
            }

            return new PassiveDamageResult(totalPhysicalDamage, totalMagicalDamage, totalTrueDamage, totalPhysicalDamageMod, totalMagicalDamageMod, totalTrueDamageMod);
        }

        public static List<DamagePassive> Passives { get; set; } = new List<DamagePassive>();

        public class PassiveDamageResult
        {
            public PassiveDamageResult(double physicalDamage, double magicalDamage, double trueDamage, double physicalDamagePercent, double magicalDamagePercent, double trueDamagePercent)
            {
                this.PhysicalDamage = Math.Floor(physicalDamage);
                this.MagicalDamage = Math.Floor(magicalDamage);
                this.TrueDamage = Math.Floor(trueDamage);

                this.PhysicalDamagePercent = physicalDamagePercent;
                this.MagicalDamagePercent = magicalDamagePercent;
                this.TrueDamagePercent = trueDamagePercent;
            }

            public double PhysicalDamage { get; set; }
            public double MagicalDamage { get; set; }
            public double TrueDamage { get; set; }
            public double PhysicalDamagePercent { get; set; }
            public double MagicalDamagePercent { get; set; }
            public double TrueDamagePercent { get; set; }
        }

        public class DamagePassive
        {
            public string Name { get; set; }

            public delegate double PassiveDamageDelegateHandler(Obj_AI_Hero source, Obj_AI_Base target);

            public PassiveDamageDelegateHandler PassiveDamage { get; set; }

            public double GetDamage(Obj_AI_Hero source, Obj_AI_Base target)
            {
                if (this.PassiveDamage != null)
                {
                    return this.PassiveDamage(source, target);
                }

                return 0;
            }

            public PassiveDamageType DamageType { get; set; }

            public enum PassiveDamageType
            {
                FlatPhysical,
                FlatMagical,
                FlatTrue,
                PercentPhysical,
                PercentMagical,
                PercentTrue
            }
        }
    }
}
