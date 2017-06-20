namespace Aimtec.SDK.Extensions
{
    using System;
    using System.Linq;

    /// <summary>
    ///     Class UnitExtensions.
    /// </summary>
    public static class UnitExtensions
    {
        #region Static Fields

        private static readonly Obj_AI_Hero Player = ObjectManager.GetLocalPlayer();

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Returns the 3D distance between two vectors.
        /// </summary>
        /// <param name="v1">The start vector.</param>
        /// <param name="v2">The end vector.</param>
        public static float Distance(this Vector3 v1, Vector3 v2)
        {
            return Vector3.Distance(v1, v2);
        }

        /// <summary>
        ///     Returns the 2D distance between two vectors.
        /// </summary>
        /// <param name="v1">The start vector.</param>
        /// <param name="v2">The end vector.</param>
        public static float Distance(this Vector2 v1, Vector2 v2)
        {
            return Vector2.Distance(v1, v2);
        }

        /// <summary>
        ///     Returns the 3D distance between a gameobject and a vector.
        /// </summary>
        /// <param name="gameObject">The GameObject.</param>
        /// <param name="v1">The vector.</param>
        public static float Distance(this GameObject gameObject, Vector3 v1)
        {
            return Vector3.Distance(gameObject.Position, v1);
        }

        /// <summary>
        ///     Returns the 3D distance between two gameobjects.
        /// </summary>
        /// <param name="gameObj">The start GameObject.</param>
        /// <param name="gameObj1">The target GameObject.</param>
        public static float Distance(this GameObject gameObj, GameObject gameObj1)
        {
            return Vector3.Distance(gameObj.Position, gameObj1.Position);
        }

        /// <summary>
        ///     Returns the 3D distance squared between two gameobjects.
        /// </summary>
        /// <param name="gameObj">The start GameObject.</param>
        /// <param name="gameObj1">The target GameObject.</param>
        public static float DistanceSqr(this GameObject gameObj, GameObject gameObj1)
        {
            return Vector3.DistanceSquared(gameObj.Position, gameObj1.Position);
        }

        /// <summary>
        ///     Returns how many stacks of the 'buffname' buff the target possesses.
        /// </summary>
        /// <param name="from">The target.</param>
        /// <param name="buffname">The buffname.</param>
        public static int GetBuffCount(this Obj_AI_Base from, string buffname)
        {
            return from.BuffManager.GetBuffCount(buffname);
        }

        /// <summary>
        ///     Gets the full attack range.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <returns>System.Single.</returns>
        public static float GetFullAttackRange(this Obj_AI_Base source, AttackableUnit target)
        {
            var baseRange = source.AttackRange + source.BoundingRadius;

            if (!target.IsValidTarget())
            {
                return baseRange;
            }

            if (Player.ChampionName.Equals("Caitlyn"))
            {
                var unit = target as Obj_AI_Base;

                if (unit != null && unit.HasBuff("caitlynyordletrapinternal"))
                {
                    baseRange = 1300 + Player.BoundingRadius;
                }
            }

            return baseRange + target.BoundingRadius;
        }
        
        /// <summary>
        ///     Counts the ally heroes in range.
        /// </summary>
        /// <param name="unit">the unit.</param>
        /// <param name="range">The range.</param>
        /// <returns>How many ally heroes are inside a 'float' range from the starting 'unit' GameObject.</returns>
        public static int CountAllyHeroesInRange(this GameObject unit, float range)
        {
            return unit.Position.CountAllyHeroesInRange(range);
        }

        /// <summary>
        ///     Counts the enemy heroes in range.
        /// </summary>
        /// <param name="unit">the unit.</param>
        /// <param name="range">The range.</param>
        /// <param name="dontIncludeStartingUnit">The original unit.</param>
        /// <returns>How many enemy heroes are inside a 'float' range from the starting 'unit' GameObject.</returns>
        public static int CountEnemyHeroesInRange(this GameObject unit, float range, Obj_AI_Base dontIncludeStartingUnit = null)
        {
            return unit.Position.CountEnemyHeroesInRange(range, dontIncludeStartingUnit);
        }

        /// <summary>
        ///     Determines whether the specified target has a determined buff.
        /// </summary>
        /// <param name="from">The target.</param>
        /// <param name="buffname">The buffname.</param>
        /// <returns>
        ///     <c>true</c> if the specified target has the 'buffname' buff; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasBuff(this Obj_AI_Base from, string buffname)
        {
            return from.Buffs.Any(b => b.IsActive && b.IsValid && string.Equals(b.Name, buffname, StringComparison.CurrentCultureIgnoreCase));
        }
        
        /// <summary>
        ///     Determines whether the specified unit is affected by a determined bufftype.
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <param name="buffType">The buff type.</param>
        /// <returns>
        ///     <c>true</c> if the specified unit is affected by the 'buffType' BuffType; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasBuffOfType(this Obj_AI_Base unit, BuffType buffType)
        {
            return unit.BuffManager.HasBuffOfType(buffType);
        }

        /// <summary>
        ///     Determines whether the specified hero target has a determined item.
        /// </summary>
        /// <param name="from">The target.</param>
        /// <param name="itemId">The item's ID.</param>
        /// <returns>
        ///     <c>true</c> if the specified hero target has the 'itemId' item; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasItem(this Obj_AI_Hero from, uint itemId)
        {
            return from.Inventory.HasItem(itemId);
        }

        /// <summary>
        ///     Determines whether the specified target has a determined item.
        /// </summary>
        /// <param name="from">The target.</param>
        /// <param name="itemId">The item's ID.</param>
        /// <returns>
        ///     <c>true</c> if the specified target has the 'itemId' item; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasItem(this Obj_AI_Base from, uint itemId)
        {
            return from.Inventory.HasItem(itemId);
        }

        /// <summary>
        ///     Returns the current health a determined unit has, in percentual.
        /// </summary>
        /// <param name="unit">The unit.</param>
        public static float HealthPercent(this Obj_AI_Base unit)
        {
            return unit.Health / unit.MaxHealth * 100;
        }
        
        /// <summary>
        ///     Returns the current mana a determined unit has, in percentual.
        /// </summary>
        /// <param name="unit">The unit.</param>
        public static float ManaPercent(this Obj_AI_Base unit)
        {
            return unit.Mana / unit.MaxMana * 100;
        }


        /// <summary>
        ///     Gets the buffs of the unit which are valid and active
        /// </summary>
        /// <param name="buff">The unit.</param>
        public static Buff[] ValidActiveBuffs(this Obj_AI_Base unit)
        {
            return unit.Buffs.Where(buff => buff.IsValid && buff.IsActive).ToArray();
        }

        /// <summary>
        ///     Returns a determined buff a determined unit has.
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <param name="buffName">The buff's stringname</param>
        public static Buff GetBuff(this Obj_AI_Base unit, string buffName)
        {
            return unit.BuffManager.GetBuff(buffName);
        }

        /// <summary>
        ///     Determines whether the specified target is inside a determined range.
        /// </summary>
        /// <param name="unit">The target.</param>
        /// <param name="range">The range.</param>
        /// <returns>
        ///     <c>true</c> if the specified target is inside the specified range; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInRange(this Obj_AI_Base unit, float range)
        {
            if (unit != null)
            {
                return Vector3.Distance(unit.Position, ObjectManager.GetLocalPlayer().Position) <= range;
            }

            return false;
        }

        /// <summary>
        ///     Determines whether the target is a valid target in the auto attack range from the specified check range from
        ///     vector.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="checkRangeFrom">The check range from.</param>
        /// <returns><c>true</c> if the target is a valid target in the auto attack range; otherwise, <c>false</c>.</returns>
        public static bool IsValidAutoRange(this AttackableUnit target, Vector3 checkRangeFrom = default(Vector3))
        {
            return target != null && target.IsValid && target.IsVisible && !target.IsDead && !target.IsInvulnerable && target.IsTargetable
                && (target.Team != ObjectManager.GetLocalPlayer().Team)
                && Vector3.Distance(target.Position, ObjectManager.GetLocalPlayer().Position)
                < Player.GetFullAttackRange((target));
        }

        /// <summary>
        ///     Determines whether the specified target is a valid target.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="range">The range.</param>
        /// <param name="allyIsValidTarget">if set to <c>true</c> allies will be set as valid targets.</param>
        /// <param name="checkRangeFrom">The check range from position.</param>
        /// <returns>
        ///     <c>true</c> if the specified target is a valid target; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValidTarget(
            this AttackableUnit target,
            float range = float.MaxValue,
            bool allyIsValidTarget = false,
            Vector3 checkRangeFrom = default(Vector3))
        {
            return target != null && target.IsValid && !target.IsDead && !target.IsInvulnerable && target.IsVisible && target.IsTargetable
                && ((allyIsValidTarget || target.Team != ObjectManager.GetLocalPlayer().Team) && Vector3.Distance(
                    target.Position,
                    ObjectManager.GetLocalPlayer().Position) < range);
        }

  

        /// <summary>
        ///     Determines whether or not the specified unit is recalling.
        /// </summary>
        /// <param name="unit">The unit</param>
        /// <returns>
        ///     <c>true</c> if the specified unit is recalling; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsRecalling(this Obj_AI_Hero unit)
        {
            return unit.ValidActiveBuffs().Any(buff => buff.Name.ToLower().Contains("recall") && buff.Type == BuffType.Aura);
        }

        #endregion
    }
}
