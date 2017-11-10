namespace Aimtec.SDK.Menu
{
    using Aimtec.SDK.Menu.Config;

    using NLog.Fluent;

    public class FontManager
    {
        #region Constants

        public const int FontSize = 16;

        #endregion

        #region Constructors and Destructors

        static FontManager()
        {
            switch (AimtecMenu.Instance["CurrentFont"].Value)
            {
                case 0:
                    CurrentFont = Arial;
                    break;
                case 1:
                    CurrentFont = Tahoma;
                    break;
                default:
                    Log.Warn().Message("Unknown font menu value").Write();
                    CurrentFont = Tahoma;
                    break;
            }
        }

        #endregion

        #region Public Properties

        public static Font Arial { get; } = new Font("Arial", FontSize, 0);

        public static Font CurrentFont { get; set; }

        public static Font Tahoma { get; } = new Font("Tahoma", FontSize, 0);

        #endregion
    }
}