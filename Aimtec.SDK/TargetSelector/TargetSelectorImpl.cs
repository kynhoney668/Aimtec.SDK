using System;
using System.Collections.Generic;
using System.Linq;
using Aimtec.SDK.Extensions;

namespace Aimtec.SDK.TargetSelector
{
    // TODO IMPLEMENT MENU SETTINGS
    internal class TargetSelectorImpl : ITargetSelector
    {
        /// <summary>
        /// Gets the target.
        /// </summary>
        /// <param name="range">The range.</param>
        /// <returns></returns>
        public Obj_AI_Hero GetTarget(float range)
        {
            return this.GetTarget(range, TargetSelectorMode.EasiestToKill);
        }

        /// <summary>
        ///     Gets the target.
        /// </summary>
        /// <param name="range">The range.</param>
        /// <param name="mode">The mode.</param>
        /// <returns></returns>
        public Obj_AI_Hero GetTarget(float range, TargetSelectorMode mode)
        {
            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .OrderByDescending(x => x, new TargetSelectorEnemyComparer {Mode = mode, Range = range})
                    .FirstOrDefault();
        }

        /// <summary>
        ///     Gets the priority.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <returns></returns>
        public static int GetPriority(Obj_AI_Hero x)
        {
            //return GetPriority(x.ChampionName);
            return 0;
        }

        /// <summary>
        ///     Gets the priority.
        /// </summary>
        /// <param name="champName">Name of the champ.</param>
        /// <returns></returns>
        public static int GetPriority(string champName)
        {
            return 0;
        }
    }

    internal class TargetSelectorEnemyComparer : IComparer<Obj_AI_Hero>
    {
        /// <summary>
        ///     Gets the player.
        /// </summary>
        /// <value>
        ///     The player.
        /// </value>
        private static Obj_AI_Hero Player => ObjectManager.GetLocalPlayer();

        /// <summary>
        ///     Gets or sets the mode.
        /// </summary>
        /// <value>
        ///     The mode.
        /// </value>
        public TargetSelectorMode Mode { get; set; }

        /// <summary>
        ///     Gets or sets the range.
        /// </summary>
        /// <value>
        ///     The range.
        /// </value>
        public float Range { get; internal set; }

        /// <summary>
        ///     Compares the specified heroes.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns></returns>
        public int Compare(Obj_AI_Hero x, Obj_AI_Hero y)
        {
            var dist1 = Player.DistanceSqr(x) - x.BoundingRadius;
            var dist2 = Player.DistanceSqr(y) - y.BoundingRadius;
            var rangeSqr = Math.Pow(this.Range, 2);


            // Both not in range
            if (dist1 > rangeSqr && dist2 > rangeSqr)
            {
                return 0;
            }

            // target 1 is in range, but target 2 isnt
            if (dist1 < rangeSqr && dist2 > rangeSqr)
            {
                return 1;
            }

            // target 1 isnt in range, but target 2 is
            if (dist1 > rangeSqr && dist2 < rangeSqr)
            {
                return -1;
            }

            // Check priority
            if (TargetSelectorImpl.GetPriority(x) < TargetSelectorImpl.GetPriority(y))
            {
                return 1;
            }

            switch (this.Mode)
            {
                case TargetSelectorMode.EasiestToKill:
                    // TODO replace this with proper damage API
                    return x.Health / Player.BaseAttackDamage < y.Health / Player.BaseAttackDamage ? 1 : -1;
                case TargetSelectorMode.Closest:
                    return x.Distance(Player) < y.Distance(Player) ? 1 : -1;
                case TargetSelectorMode.NearMouse:
                    return x.Distance(Game.CursorPos) < y.Distance(Game.CursorPos) ? 1 : -1;
                case TargetSelectorMode.MostAp:
                    return x.BaseAbilityDamage < y.BaseAbilityDamage ? 1 : -1;
                case TargetSelectorMode.MostAd:
                    return x.BaseAttackDamage < y.BaseAttackDamage ? 1 : -1;
                case TargetSelectorMode.LowestHealth:
                    return x.Health < y.Health ? 1 : -1;
                default:
                    return 0;
            }
        }
    }
}