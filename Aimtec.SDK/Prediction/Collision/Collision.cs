namespace Aimtec.SDK.Prediction.Collision
{
    using System;
    using System.Linq;

    using Aimtec.SDK.Extensions;
    using Aimtec.SDK.Prediction.Skillshots;

    /// <summary>
    ///     Class Collision.
    /// </summary>
    public class Collision
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Checks the collision.
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <param name="position">The position.</param>
        /// <param name="delay">The delay.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="range">The range.</param>
        /// <param name="speed">The speed.</param>
        /// <param name="from">From.</param>
        /// <returns><c>true</c> if there are minions that will collide with the projectile, <c>false</c> otherwise.</returns>
        public static bool CheckCollision(
            Obj_AI_Base unit,
            Vector3 position,
            float delay,
            float radius,
            float range,
            float speed,
            Vector3 from)
        {
            return GameObjects.EnemyMinions
                              .Where(
                                  x => Vector3.DistanceSquared(x.Position, from)
                                      <= range + 500 * (delay + range / speed))
                              .Any(x => WillCollideWith(unit, x, position, delay, radius, range, speed, from));
        }

        /// <summary>
        ///     Checks the collision.
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <param name="minion">The minion.</param>
        /// <param name="position">The position.</param>
        /// <param name="delay">The delay.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="range">The range.</param>
        /// <param name="speed">The speed.</param>
        /// <param name="from">From.</param>
        /// <returns><c>true</c> if the projectile will collide with the <paramref name="minion" />, <c>false</c> otherwise.</returns>
        public static bool WillCollideWith( // todo yasuo E, braum W
            Obj_AI_Base unit,
            Obj_AI_Base minion,
            Vector3 position,
            float delay,
            float radius,
            float range,
            float speed,
            Vector3 from)
        {
            if (unit.NetworkId == minion.NetworkId)
            {
                return false;
            }

            // todo check if minion will die before missile hits

            var waypoints = minion.Path.Select(x => (Vector2) x);
            var mpos = (Vector2) (waypoints.Any()
                ? Prediction.Instance.CalculateTargetPosition(minion, delay, radius, speed, from, SkillType.Line)
                            .UnitPosition
                : minion.Position);

            if (!(Vector2.DistanceSquared((Vector2) from, mpos) <= range * range)
                || !(Vector2.DistanceSquared((Vector2) from, (Vector2) minion.Position) <= Math.Pow(range + 100, 2)))
            {
                return false;
            }

            var buffer = 8;

            if (minion.Type == GameObjectType.AIHeroClient)
            {
                buffer += (int) minion.BoundingRadius; // todo make sure this is ~65
            }

            var from2D = (Vector2) from;

            if (minion.Path.Length > 1)
            {
                var vppol = from2D.ProjectOn((Vector2) position, mpos);

                if (vppol.IsOnSegment && Vector2.DistanceSquared(mpos, vppol.SegmentPoint)
                    <= Math.Pow(minion.BoundingRadius + radius + buffer, 2))
                {
                    return true;
                }
            }

            var vppol2 = from2D.ProjectOn((Vector2) position, (Vector2) minion.Position);

            return vppol2.IsOnSegment && Vector2.DistanceSquared((Vector2) minion.Position, vppol2.SegmentPoint)
                <= Math.Pow(minion.BoundingRadius + radius + buffer, 2);
        }

        #endregion
    }
}