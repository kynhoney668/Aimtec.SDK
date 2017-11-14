namespace Aimtec.SDK.Menu.Config
{
    using Aimtec.SDK.Menu.Components;
    using Aimtec.SDK.Util;

    using NLog.Fluent;

    internal class AimtecMenu : Menu
    {
        #region Constructors and Destructors

        internal AimtecMenu()
            : base("Aimtec.Menu", "Aimtec", true)
        {
            Hacks.Init(this);

            this.Add(new MenuBool("Aimtec.Debug", "Debugging", false, true));

            Log.Info().Message("Aimtec menu created").Write();
        }

        #endregion

        #region Properties

        internal static bool DebugEnabled => Instance["Aimtec.Debug"].Enabled;

        internal static AimtecMenu Instance { get; } = new AimtecMenu();

        #endregion
    }
}