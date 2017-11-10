namespace Aimtec.SDK.Menu.Config
{
    using Aimtec.SDK.Menu.Components;

    using NLog.Fluent;

    internal class AimtecMenu : Menu
    {
        #region Constructors and Destructors

        internal AimtecMenu()
            : base("Aimtec.Menu", "Aimtec", true)
        {
            this.Add(new MenuList("CurrentFont", "Menu Font", new[] { "Arial", "Tahoma", "Calibri", "Segoe UI" }, 2, true));
            this.Add(new MenuBool("Aimtec.Debug", "Debugging", false, true));

            this["CurrentFont"].OnValueChanged += (sender, args) =>
            {
                var val = args.GetNewValue<MenuList>();

                switch (val.Value)
                {
                    case 0:
                        FontManager.CurrentFont = FontManager.Arial;
                        break;
                    case 1:
                        FontManager.CurrentFont = FontManager.Tahoma;
                        break;
                    case 2:
                        FontManager.CurrentFont = FontManager.Calibri;
                        break;
                    case 3:
                        FontManager.CurrentFont = FontManager.SegoeUI;
                        break;
                    default:
                        Log.Warn().Message("Unknown font menu value").Write();
                        break;
                }
            };

            Log.Info().Message("Aimtec menu created").Write();
        }

        #endregion

        #region Properties

        internal static bool DebugEnabled => Instance["Aimtec.Debug"].Enabled;

        internal static AimtecMenu Instance { get; } = new AimtecMenu();

        #endregion
    }
}