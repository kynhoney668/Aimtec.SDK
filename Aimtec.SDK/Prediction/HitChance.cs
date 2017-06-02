namespace Aimtec.SDK.Prediction
{
    /// <summary>
    ///     The chance that the prediction will hit the target.
    /// </summary>
    public enum HitChance
    {
        /// <summary>
        ///     The chance to hit the target is impossible.
        /// </summary>
        Impossible = 0,

        /// <summary>
        ///     The chance to hit the target is low.
        /// </summary>
        Low = 1,

        /// <summary>
        ///     The target may or may not get hit.
        /// </summary>
        Medium = 2,

        /// <summary>
        ///     The target has a high chance of being hit.
        /// </summary>
        High = 3,

        /// <summary>
        ///     The target is dashing.
        /// </summary>
        Dashing = 4,

        /// <summary>
        ///     The target is immobile.
        /// </summary>
        Immobile = 5,

        /// <summary>
        ///     The target is out of range.
        /// </summary>
        OutOfRange = 6,

        /// <summary>
        ///      Collision between the target and the skillshot.
        /// </summary>
        Collision = 7
    }
}