namespace Aimtec.SDK.TargetSelector
{
    public enum TargetSelectorMode
    {
        /// <summary>
        /// Sorts target by how easy they are to kill with spells.
        /// </summary>
        EasiestToKill = 1,

        /// <summary>
        /// Gets the target closest to the player.
        /// </summary>
        Closest = 2,

        /// <summary>
        /// Gets the target nearest to the mouse.
        /// </summary>
        NearMouse = 3,

        /// <summary>
        /// Gets the target with the most ability power.
        /// </summary>
        MostAp = 4,
        /// <summary>
        ///  Gets the target with the most attack damage.
        /// </summary>
        MostAd = 5,
        /// <summary>
        /// Gets the target with the least health.
        /// </summary>
        LowestHealth = 6
    }
}