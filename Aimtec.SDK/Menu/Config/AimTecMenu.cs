namespace Aimtec.SDK.Menu.Config
{
    using NLog.Fluent;

    internal class AimtecMenu : Menu
    {
        internal static AimtecMenu Instance { get; } = new AimtecMenu();

        internal AimtecMenu() : base("Aimtec.Menu", "Aimtec", true)
        {
            Log.Info().Message("Aimtec menu created").Write();
        }
    }
}
