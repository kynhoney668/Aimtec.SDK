namespace Aimtec.SDK.TargetSelector
{
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
    }
}