namespace Aimtec.SDK.Menu.SDKMenuInstance
{
    class AimTecMenu : Menu
    {
        internal static AimTecMenu Instance { get; } = new AimTecMenu();

        internal AimTecMenu() : base("Aimtec.Menu", "Aimtec", true)
        {
        }
    }
}
