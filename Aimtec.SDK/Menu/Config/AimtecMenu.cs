namespace Aimtec.SDK.Menu.Config
{
    using Aimtec.SDK.Menu.Components;
    using NLog.Fluent;

    internal class AimtecMenu : Menu
    {
        internal static AimtecMenu Instance { get; } = new AimtecMenu();

        internal AimtecMenu() : base("Aimtec.Menu", "Aimtec", true)
        {
            this.Add(new MenuBool("Aimtec.Debug", "Debugging", false, true));

            Log.Info().Message("Aimtec menu created").Write();
        }

        internal static bool DebugEnabled => Instance["Aimtec.Debug"].Enabled;
    }
}
