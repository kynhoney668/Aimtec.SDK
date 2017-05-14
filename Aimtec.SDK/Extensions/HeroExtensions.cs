using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Aimtec.SDK.Extensions
{
    public static class HeroExtensions
    {

        /// <summary>
        /// Determines whether the specified target is a valid target.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="range">The range.</param>
        /// <param name="allyIsValidTarget">if set to <c>true</c> allies will be set as valid targets.</param>
        /// <param name="checkRangeFrom">The check range from position.</param>
        /// <returns>
        ///   <c>true</c> if the specified target is a valid target; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValidTarget(this Obj_AI_Base target, float range = float.MaxValue, bool allyIsValidTarget = false, Vector3 checkRangeFrom = default(Vector3))
        {
            return target != null && !target.IsDead && !target.IsInvulnerable && target.IsVisible && target.IsTargetable &&
                   ((allyIsValidTarget || target.Team != ObjectManager.GetLocalPlayer().Team) &&
                    Vector3.Distance(target.Position, ObjectManager.GetLocalPlayer().Position) < range);
        }

        public static float Distance(this Vector3 v1, Vector3 v2)
        {
            return Vector3.Distance(v1, v2);
        }

        public static float Distance(this Vector2 v1, Vector2 v2)
        {
            return Vector2.Distance(v1, v2);
        }

        public static float Distance(this GameObject gameObject, Vector3 v1)
        {
            return Vector3.Distance(gameObject.Position, v1);
        }

        public static float Distance(this GameObject gameObj, GameObject gameObj1)
        {
            return Vector3.Distance(gameObj.Position, gameObj1.Position);
        }

        public static float DistanceSqr(this GameObject gameObj, GameObject gameObj1)
        {
            return Vector3.DistanceSquared(gameObj.Position, gameObj1.Position);
        }

        public static bool IsMelee(this Obj_AI_Base gameObject)
        {
            // TODO Replace with proper API once available.
            return gameObject.AttackRange > 300;
        }

        public static int GetBuffCount(this Obj_AI_Base from, string buffname)
        {     
            return from.BuffManager.GetBuffCount(buffname);
        }

        public static bool HasBuff(this Obj_AI_Base from, string buffname)
        {
            return from.BuffManager.HasBuff(buffname);
        }
    }
}
