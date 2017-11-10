namespace Aimtec.SDK.Menu
{
    using Aimtec.SDK.Menu.Config;

    using NLog.Fluent;

    public class FontManager
    {
        #region Constructors and Destructors

        static FontManager()
        {
            UpdateFonts(false);

            switch (AimtecMenu.Instance["CurrentFont"].Value)
            {
                case 0:
                    CurrentFont = Arial;
                    break;
                case 1:
                    CurrentFont = Tahoma;
                    break;
                case 2:
                    CurrentFont = Calibri;
                    break;
                case 3:
                    CurrentFont = SegoeUI;
                    break;
                default:
                    Log.Warn().Message("Unknown font menu value").Write();
                    CurrentFont = Tahoma;
                    break;
            }

            FontSize = AimtecMenu.Instance["FontSize"].Value;
        }

        #endregion

        #region Public Properties

        public static Font Arial { get; private set; }

        public static Font Calibri { get; private set; }

        public static Font CurrentFont { get; set; }

        public static int FontSize { get; internal set; }

        public static Font SegoeUI { get; private set; }

        public static Font Tahoma { get; private set; }

        #endregion

        #region Methods

        internal static void UpdateFonts(bool updateCurrent = true)
        {
            Arial = CreateFont("Arial");
            Tahoma = CreateFont("Tahoma");
            Calibri = CreateFont("Calibri");
            SegoeUI = CreateFont("Segoe UI");

            if (updateCurrent)
            {
                CurrentFont = CreateFont(CurrentFont.Facename);
            }
        }

        private static Font CreateFont(string name)
        {
            return new Font(name, FontSize, 0) { Quality = FontQuality.AntiAliased };
        }

        #endregion
    }
}