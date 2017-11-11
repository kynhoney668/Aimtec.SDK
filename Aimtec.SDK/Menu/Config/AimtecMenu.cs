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
            this.Add(new MenuSlider("FontSize", "Font Size", 16, 10, 20, true));
            this.Add(new MenuBool("Aimtec.Debug", "Debugging", false, true));

            this["CurrentFont"].OnValueChanged += (sender, args) =>
            {
                FontManager.CurrentFont = FontManager.GetFontById(args.GetNewValue<MenuList>().Value);
            };

            this["FontSize"].OnValueChanged += (sender, args) =>
            {
                FontManager.FontSize = args.GetNewValue<MenuSlider>().Value;
                FontManager.UpdateFonts();
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