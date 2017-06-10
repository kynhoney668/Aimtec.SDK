namespace Aimtec.SDK.TargetSelector
{
    using Aimtec.SDK.Menu;

    /// <summary>
    /// The base class for target selector.
    /// </summary>
    public interface ITargetSelector
    {
        /// <summary>
        /// Gets the target.
        /// </summary>
        /// <param name="range">The range.</param>
        /// <returns></returns>
        Obj_AI_Hero GetTarget(float range);

        /// <summary>
        /// Gets the target for the Orbwalker to attack
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        Obj_AI_Hero GetOrbwalkingTarget();
    
    }
}