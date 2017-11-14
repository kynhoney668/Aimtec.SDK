namespace Aimtec.SDK.Util
{
    using Aimtec.SDK.Menu;
    using Aimtec.SDK.Menu.Components;

    public class Hacks
    {
        #region Public Properties

        public static bool EnableDrawing
        {
            get => Feature.Drawing;
            set => Feature.Drawing = value;
        }

        #endregion

        #region Methods

        internal static void Init(Menu root)
        {
            var menu = new Menu("Aimtec.Menu.Hacks", "Hacks");

            var enableDrawing = new MenuBool("EnableDrawing", "Enable Drawing", shared: true);
            enableDrawing.SetToolTip("Disables ALL drawings untill the Re-enable key is pressed.");
            enableDrawing.OnValueChanged += (sender, args) => EnableDrawing = args.GetNewValue<MenuBool>().Value;

            var enabler = new MenuKeyBind(
                "ReenableDrawing",
                "Re-enable Drawings",
                KeyCode.F10,
                KeybindType.Press,
                shared: true).SetToolTip("Re-Enables drawings.");

            enabler.OnValueChanged += (sender, args) =>
            {
                // Doesn't fire OnValueChanged
                enableDrawing.Value = true;
                EnableDrawing = true;
            };

            // Loads value from config
            menu.Add(enableDrawing);
            menu.Add(enabler);

            root.Add(menu);

            // Sets drawings from config
            EnableDrawing = enableDrawing.Value;
        }

        #endregion
    }
}