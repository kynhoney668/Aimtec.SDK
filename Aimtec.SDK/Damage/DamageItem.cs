
// ReSharper disable ConvertToLambdaExpression
// ReSharper disable LoopCanBeConvertedToQuery

namespace Aimtec.SDK.Damage
{
    using Aimtec.SDK.Extensions;

    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class DamageItem
    {
        static DamageItem()
        {
            #region Physical Damage Items

            Items.Add(new Item
                          {
                              Id = ItemId.BladeoftheRuinedKing,
                              DamageType = Item.ItemDamageType.Physical,
                              ItemPhysicalDamage = (source, target) =>
                                  {
                                      switch (target.Type)
                                      {
                                          case GameObjectType.obj_AI_Minion:
                                              return Math.Min(0.08 * target.Health, 60);

                                          case GameObjectType.obj_AI_Hero:
                                              return Math.Max(0.08 * target.Health, 15);
                                      }

                                      return 0;
                                  }
                          });

            Items.Add(new Item
                          {
                              Id = ItemId.EnchantmentBloodrazor,
                              DamageType = Item.ItemDamageType.Physical,
                              ItemPhysicalDamage = (source, target) =>
                                  {
                                      switch (target.Type)
                                      {
                                          case GameObjectType.obj_AI_Minion:
                                              return Math.Min(0.04 * target.MaxHealth, 75);
                                          
                                          case GameObjectType.obj_AI_Hero:
                                              return 0.04 * target.MaxHealth;
                                      }

                                      return 0;
                                  }
                          });

            Items.Add(new Item
                          {
                              Id = ItemId.RecurveBow,
                              DamageType = Item.ItemDamageType.Physical,
                              ItemPhysicalDamage = (source, target) =>
                                  {
                                      return 15;
                                  }
                          });


            Items.Add(new Item
                          {
                              Id = ItemId.TitanicHydra,
                              DamageType = Item.ItemDamageType.Physical,
                              ItemPhysicalDamage = (source, target) =>
                                  {
                                      if (source.HasBuff("itemtitanichydracleavebuff"))
                                      {
                                          return 40 + 0.1 * source.MaxHealth;
                                      }

                                      return 5 + 0.01 * source.MaxHealth;
                                  }
                          });

            Items.Add(new Item
                          {
                              Id = ItemId.TrinityForce,
                              DamageType = Item.ItemDamageType.Physical,
                              ItemPhysicalDamage = (source, target) =>
                                  {
                                      if (source.HasBuff("sheen"))
                                      {
                                          return source.BaseAttackDamage * 2;
                                      }

                                      return 0;
                                  }
                          });

            Items.Add(new Item
                          {
                              Id = ItemId.IcebornGauntlet,
                              DamageType = Item.ItemDamageType.Physical,
                              ItemPhysicalDamage = (source, target) =>
                                  {
                                      if (source.HasBuff("sheen"))
                                      {
                                          return source.BaseAttackDamage;
                                      }

                                      return 0;
                                  }
                          });

            Items.Add(new Item
                          {
                              Id = ItemId.Sheen,
                              DamageType = Item.ItemDamageType.Physical,
                              ItemPhysicalDamage = (source, target) =>
                                  {
                                      if (source.HasBuff("sheen"))
                                      {
                                          return source.BaseAttackDamage;
                                      }

                                      return 0;
                                  }
                          });

            Items.Add(new Item
                          {
                              Id = ItemId.Muramana,
                              DamageType = Item.ItemDamageType.Physical,
                              ItemPhysicalDamage = (source, target) =>
                                  {
                                      if (source.ManaPercent() > 20)
                                      {
                                          return 0.06 * source.Mana;
                                      }

                                      return 0;
                                  }
                          });

            #endregion

            #region Magical Damage Items

            Items.Add(new Item
                          {
                              Id = ItemId.GuinsoosRageblade,
                              DamageType = Item.ItemDamageType.Magical,
                              ItemMagicalDamage = (source, target) =>
                                  {
                                      return 15;
                                  }
                          });

            Items.Add(new Item
                          {
                              Id = ItemId.NashorsTooth,
                              DamageType = Item.ItemDamageType.Magical,
                              ItemMagicalDamage = (source, target) =>
                                  {
                                      return 15 + 0.15 * source.TotalAbilityDamage;
                                  }
                          });

            Items.Add(new Item
                          {
                              Id = ItemId.WitsEnd,
                              DamageType = Item.ItemDamageType.Magical,
                              ItemMagicalDamage = (source, target) =>
                                  {
                                      return 40;
                                  }
                          });

            Items.Add(new Item
                          {
                              Id = ItemId.LichBane,
                              DamageType = Item.ItemDamageType.Magical,
                              ItemMagicalDamage = (source, target) =>
                                  {
                                      return source.BaseAttackDamage + 0.50 * source.TotalAbilityDamage;
                                  }
                          });

            Items.Add(new Item
                          {
                              Id = ItemId.KircheisShard,
                              DamageType = Item.ItemDamageType.Magical,
                              ItemMagicalDamage = (source, target) =>
                                  {
                                      if (source.GetBuffCount("ItemStatikShankCharge") == 100)
                                      {
                                          return 50;
                                      }

                                      return 0;
                                  }
                          });

            Items.Add(new Item
                          {
                              Id = ItemId.RapidFirecannon,
                              DamageType = Item.ItemDamageType.Magical,
                              ItemMagicalDamage = (source, target) =>
                                  {
                                      if (source.GetBuffCount("ItemStatikShankCharge") == 100)
                                      {
                                          return new[] { 50, 50, 50, 50, 50, 50, 56, 67, 72, 77, 83, 88, 94, 99, 104, 110, 115, 120 }[source.Level - 1];
                                      }

                                      return 0;
                                  }
                          });

            Items.Add(new Item
                          {
                              Id = ItemId.StatikkShiv,
                              DamageType = Item.ItemDamageType.Magical,
                              ItemMagicalDamage = (source, target) =>
                                  {
                                      if (source.GetBuffCount("ItemStatikShankCharge") == 100)
                                      {
                                          return new[] { 60, 60, 60, 60, 60, 68, 76, 84, 91, 99, 107, 114, 122, 130, 137, 145, 153, 160 }[source.Level - 1];
                                      }

                                      return 0;
                                  }
                          });

            #endregion
        }

        public static ItemDamageResult ComputeItemDamages(Obj_AI_Base source, Obj_AI_Base target)
        {
            double totalPhysicalDamage = 0;
            double totalMagicalDamage = 0;
            double totalTrueDamage = 0;
            
            foreach (var item in Items)
            {
                if (!source.HasItem(item.Id))
                {
                    continue;
                }

                if (item.DamageType.HasFlag(Item.ItemDamageType.Physical))
                {
                    totalPhysicalDamage += item.GetPhysicalDamage(source, target);
                }


                if (item.DamageType.HasFlag(Item.ItemDamageType.Magical))
                {
                    totalMagicalDamage += item.GetMagicalDamage(source, target);
                }

                if (item.DamageType.HasFlag(Item.ItemDamageType.True))
                {
                    totalTrueDamage += item.GetTrueDamage(source, target);
                }
            }

            return new ItemDamageResult(totalPhysicalDamage, totalMagicalDamage, totalTrueDamage);
        }

        public static List<Item> Items { get; set; } = new List<Item>();

        public class ItemDamageResult
        {
            public ItemDamageResult(double physicalDamage, double magicalDamage, double trueDamage)
            {
                this.PhysicalDamage = physicalDamage;
                this.MagicalDamage = magicalDamage;
                this.TrueDamage = trueDamage;
            }

            public double PhysicalDamage { get; set; }
            public double MagicalDamage { get; set; }
            public double TrueDamage { get; set; }
        }

        public class Item
        {
            public uint Id { get; set; }

            public delegate double ItemDamageDelegateHandler(Obj_AI_Base source, Obj_AI_Base target);

            public ItemDamageDelegateHandler ItemPhysicalDamage { get; set; }
            public ItemDamageDelegateHandler ItemMagicalDamage { get; set; }
            public ItemDamageDelegateHandler ItemTrueDamage { get; set; }

            public double GetPhysicalDamage(Obj_AI_Base source, Obj_AI_Base target)
            {
                if (this.ItemPhysicalDamage != null)
                {
                    return this.ItemPhysicalDamage(source, target);
                }

                return 0;
            }

            public double GetMagicalDamage(Obj_AI_Base source, Obj_AI_Base target)
            {
                if (this.ItemMagicalDamage != null)
                {
                    return this.ItemMagicalDamage(source, target);
                }

                return 0;
            }

            public double GetTrueDamage(Obj_AI_Base source, Obj_AI_Base target)
            {
                if (this.ItemTrueDamage != null)
                {
                    return this.ItemTrueDamage(source, target);
                }

                return 0;
            }

            public ItemDamageType DamageType { get; set; }

            [Flags]
            public enum ItemDamageType
            {
                Physical = 0x1,
                Magical = 0x2,
                True = 0x4
            }
        }
    }
}
