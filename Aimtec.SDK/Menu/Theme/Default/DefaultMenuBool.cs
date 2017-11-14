namespace Aimtec.SDK.Menu.Theme.Default
{
    using System.Drawing;

    using Aimtec.SDK.Menu.Components;

    internal class DefaultMenuBool : IRenderManager<MenuBool, DefaultMenuTheme>
    {
        #region Constructors and Destructors

        public DefaultMenuBool(MenuBool component, DefaultMenuTheme theme)
        {
            this.Component = component;
            this.Theme = theme;
        }

        #endregion

        #region Public Properties

        public MenuBool Component { get; }

        public DefaultMenuTheme Theme { get; }

        #endregion

        #region Public Methods and Operators

        public void Render(Vector2 pos)
        {
            var width = this.Component.Parent.Width;
            var height = MenuManager.MaxHeightItem + this.Theme.BonusMenuHeight;

            this.Theme.DrawMenuItemBorder(pos, width);

            var position = pos + this.Theme.LineWidth;

            this.Theme.DrawMenuItemBox(position, width);

            var leftVCenter = RenderTextFlags.VerticalCenter | RenderTextFlags.HorizontalLeft | RenderTextFlags.NoClip
                              | RenderTextFlags.SingleLine;


            var displayNamePosition = new Aimtec.Rectangle((int)position.X + this.Theme.TextSpacing, (int)position.Y, (int)(position.X + width), (int)(position.Y + height));

            FontManager.CurrentFont.Draw(this.Component.DisplayName + (!string.IsNullOrEmpty(this.Component.ToolTip) ? " [?]" : ""),
                displayNamePosition,
                leftVCenter, this.Theme.TextColor);

            // Render indicator box outline
            Aimtec.Render.Line(
                pos.X + width - this.Theme.IndicatorWidth - this.Theme.LineWidth,
                pos.Y,
                pos.X + width - this.Theme.IndicatorWidth - this.Theme.LineWidth,
                pos.Y + height,
                Color.FromArgb(82, 83, 57));

            // Draw indicator box
            var indBoxPosition = position + new Vector2(width - this.Theme.IndicatorWidth - this.Theme.LineWidth, 0);

            var boolColor = this.Component.Value ? Color.FromArgb(39, 96, 17) : Color.FromArgb(85, 25, 15);

            Aimtec.Render.Rectangle(
                indBoxPosition,
                this.Theme.IndicatorWidth,
                height - this.Theme.LineWidth,
                boolColor);

            var centerArrowBox = new Aimtec.Rectangle(
                (int)indBoxPosition.X,
                (int)indBoxPosition.Y,
                (int)(indBoxPosition.X + this.Theme.IndicatorWidth),
                (int)indBoxPosition.Y + height);


            FontManager.CurrentFont.Draw(this.Component.Value ? "ON" : "OFF",
                centerArrowBox,
                RenderTextFlags.HorizontalCenter | RenderTextFlags.VerticalCenter, Color.White);
        }

        #endregion
    }
}