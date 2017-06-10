using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aimtec.SDK.Prediction.Collision
{
    /// <summary>
    /// Enum CollisionableObjects
    /// </summary>
    public enum CollisionableObjects
    {
        /// <summary>
        ///     Minion Collision-able Flag
        /// </summary>
        Minions = 1 << 0,

        /// <summary>
        ///     Hero Collision-able Flag
        /// </summary>
        Heroes = 1 << 1,

        /// <summary>
        ///     Yasuo's Wall Collision-able Flag
        /// </summary>
        YasuoWall = 1 << 2,

        /// <summary>
        ///     Braum's Shield Collision-able Flag
        /// </summary>
        BraumShield = 1 << 3,

        /// <summary>
        ///     Wall Collision-able Flag
        /// </summary>
        Walls = 1 << 4
    }
}
