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
        /// <returns></returns>
        Obj_AI_Hero GetTarget(float range);

        /// <summary>
        /// Gets the target.
        /// </summary>
        /// <param name="range">The range.</param>
        /// <returns></returns>
        IOrderedEnumerable<Obj_AI_Hero> GetOrderedTargets(float range);

        /// <summary>
        /// Gets the target for the Orbwalker to attack
        /// </summary>
        /// <returns></returns>
        Obj_AI_Hero GetOrbwalkingTarget();

    }
}