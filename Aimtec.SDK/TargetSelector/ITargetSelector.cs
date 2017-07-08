namespace Aimtec.SDK.TargetSelector
{
    using System;

    using Aimtec.SDK.Menu;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The base class for target selector.
    /// </summary>
    public interface ITargetSelector : IDisposable
    {
        /// <summary>
        ///     Gets or sets the Target Selector Menu
        /// </summary>
        Menu Config { get; set; }

        /// <summary>
        /// Gets the target.
        /// </summary>
        /// <param name="range">The range.</param>
        /// <param name="autoAttackTarget">If true then range is ignored and max auto attack range is used</param>
        /// <returns></returns>
        Obj_AI_Hero GetTarget(float range, bool autoAttackTarget);

        /// <summary>
        /// Gets the target.
        /// </summary>
        /// <param name="range">The range.</param>
        /// <param name="autoAttackTarget">If true then range is ignored and max auto attack range is used</param>
        /// <returns></returns>
        List<Obj_AI_Hero> GetOrderedTargets(float range, bool autoAttackTarget);

    }
}