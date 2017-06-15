using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aimtec.SDK.Extensions
{
    public static class Vector3Extensions
    {
        /// <summary>
        /// Normalizeds the specified vector.
        /// </summary>
        /// <param name="v">The vector.</param>
        /// <returns>Vector3.</returns>
        public static Vector3 Normalized(this Vector3 v)
        {
            return Vector3.Normalize(v);
        }

        public static Vector2 To2D(this Vector3 v)
        {
            return (Vector2) v;
        }

        public static bool PointUnderEnemyTurret(this Vector3 point)
        {
            var enemyTurrets = ObjectManager.Get<Obj_AI_Turret>().Any(t => t.IsEnemy && point.Distance(t.Position) < 950f + ObjectManager.GetLocalPlayer().BoundingRadius + t.BoundingRadius);
            return enemyTurrets;
        }

        public static bool PointUnderAllyTurret(this Vector3 point)
        {
            var allyTurrets = ObjectManager.Get<Obj_AI_Turret>().Any(t => t.IsAlly && point.Distance(t.Position) < 950f + ObjectManager.GetLocalPlayer().BoundingRadius + t.BoundingRadius);
            return allyTurrets;
        }
        
        /// <summary>
        ///     Extends a Vector3 to another Vector3.
        /// </summary>
        /// <param name="vector3">the starting position.</param>
        /// <param name="toVector3">the target direction.</param>
        /// <param name="distance">the amount of distance.</param>
        /// <returns>'vector3' extended to 'toVector3' by 'distance'.</returns>
        public static Vector3 Extend(this Vector3 vector3, Vector3 toVector3, float distance)
        {
            return vector3 + distance * (toVector3 - vector3).Normalized();
        }

        /// <summary>
        ///     Extends a Vector3 to a Vector2.
        /// </summary>
        /// <param name="vector3">the starting position.</param>
        /// <param name="toVector2">the target direction.</param>
        /// <param name="distance">the amount of distance.</param>
        /// <returns>'vector3' extended to 'toVector2' by 'distance'.</returns>
        public static Vector3 Extend(this Vector3 vector3, Vector2 toVector2, float distance)
        {
            return vector3 + distance * ((Vector3)toVector2 - vector3).Normalized();
        }
    }
}
