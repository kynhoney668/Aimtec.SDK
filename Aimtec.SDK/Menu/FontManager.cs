namespace Aimtec.SDK.Menu
{
    using Aimtec.SDK.Menu.Config;

    using NLog.Fluent;

    public class FontManager
    {
        #region Constructors and Destructors

        static FontManager()
        {
            FontSize = AimtecMenu.Instance["FontSize"].Value;
            UpdateFonts(false);
            CurrentFont = GetFontById(AimtecMenu.Instance["CurrentFont"].Value);
        }

        #endregion

        #region Public Properties

        public static Font CurrentFont { get; set; }

        public static int FontSize { get; internal set; }
    
        public static Font Arial { get; private set; }

        public static Font Calibri { get; private set; }

        public static Font SegoeUI { get; private set; }

        public static Font Tahoma { get; private set; }

        #endregion

        #region Methods

        internal static Font GetFontById(int id)
        {
            switch (id)
            {
                case 0:
                    return Arial;
                case 1:
                    return Tahoma;
                case 2:
                    return Calibri;
                case 3:
                    return SegoeUI;
                default:
                    Log.Warn().Message("Unknown font menu value").Write();
                    return Tahoma;
            }
        }

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