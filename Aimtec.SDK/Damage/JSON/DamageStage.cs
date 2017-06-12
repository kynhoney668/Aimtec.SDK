namespace Aimtec.SDK.Damage.JSON
{
    /// <summary>
    ///     Enum DamageStage
    /// </summary>
    public enum DamageStage
    {
        /// <summary>
        ///     The default damage stage.
        /// </summary>
        Default,

        /// <summary>
        ///     The way back damage stage.
        /// </summary>
        WayBack,

        /// <summary>
        ///     The detonation damage stage.
        /// </summary>
        Detonation,

        /// <summary>
        ///     The damage per second stage.
        /// </summary>
        DamagePerSecond,

        /// <summary>
        ///     The second form damage stage.
        /// </summary>
        SecondForm,
        
        /// <summary>
        ///     The third form damage stage.
        /// </summary>
        ThirdForm,

        /// <summary>
        ///     The second cast damage stage.
        /// </summary>
        SecondCast,
        
        /// <summary>
        ///     The third cast damage stage.
        /// </summary>
        ThirdCast,
        
        /// <summary>
        ///     The against minion damage stage.
        /// </summary>
        AgainstMinion,

        /// <summary>
        ///     The buff damage stage.
        /// </summary>
        Buff,

        /// <summary>
        ///     The AoE damage stage.
        /// </summary>
        AreaOfEffect,
        
        /// <summary>
        ///     The collision damage stage.
        /// </summary>
        Collision,

        /// <summary>
        ///     The empowered damage stage.
        /// </summary>
        Empowered,

        OverTime // todo what the fuck is this (blitzcrank R)
    }
}
