namespace Aimtec.SDK.TargetSelector
{
    using Aimtec.SDK.Menu;

    /// <summary>
    ///     Selects the best enemy to target.
    /// </summary>
    public class TargetSelector : ITargetSelector
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TargetSelector" /> class.
        /// </summary>
        /// <param name="impl">The implementation.</param>
        public TargetSelector(ITargetSelector impl)
        {
            Implementation = impl;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TargetSelector" /> class.
        /// </summary>
        public TargetSelector()
        {
        }

        /// <summary>
        ///     Gets or sets the implementation of the target selector.
        /// </summary>
        /// <value>
        ///     The implementation of the target selector.
        /// </value>
        public static ITargetSelector Implementation { get; set; } = new TargetSelectorImpl();

        /// <summary>
        ///     Gets the target.
        /// </summary>
        /// <param name="range">The range.</param>
        /// <returns></returns>
        public Obj_AI_Hero GetTarget(float range)
        {
            return Implementation.GetTarget(range);
        }

        public Obj_AI_Hero GetOrbwalkingTarget()
        {
            return Implementation.GetOrbwalkingTarget();
        }

    }
}