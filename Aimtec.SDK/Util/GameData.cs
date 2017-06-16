namespace Aimtec.SDK.Util
{
    using System;

    /// <summary>
    ///     Class GameData.
    /// </summary>
    public class GameData
    {
        #region Public Properties

        /// <summary>
        ///     Gets the tick count.
        /// </summary>
        /// <value>The tick count.</value>
        public static int TickCount => Environment.TickCount & int.MaxValue;

        #endregion
    }
}