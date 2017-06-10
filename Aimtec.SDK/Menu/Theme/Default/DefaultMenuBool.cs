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

            this.Theme.DrawMenuItemBorder(pos, width);

            var position = pos + DefaultMenuTheme.LineWidth;

            this.Theme.DrawMenuItemBox(position, width);

            var centerPoint = pos + new Vector2(width - (DefaultMenuTheme.LineWidth * 2) / 2, this.Theme.MenuHeight - (DefaultMenuTheme.LineWidth * 2) / 2);

            var displayNamePosition = position + new Vector2(DefaultMenuTheme.TextSpacing, (this.Theme.MenuHeight) / 2);

            RenderManager.RenderText(
                displayNamePosition,
                Color.FromArgb(207, 195, 149),
                this.Component.DisplayName, RenderTextFlags.VerticalCenter);

            // Render indicator box outline
            RenderManager.RenderLine(
                pos.X + width - DefaultMenuTheme.IndicatorWidth - DefaultMenuTheme.LineWidth,
                pos.Y,
                pos.X + width - DefaultMenuTheme.IndicatorWidth - DefaultMenuTheme.LineWidth,
                pos.Y + this.Theme.MenuHeight,
                Color.FromArgb(82, 83, 57));

            // Draw indicator box
            var indBoxPosition = position + new Vector2(width - DefaultMenuTheme.IndicatorWidth - DefaultMenuTheme.LineWidth, 0);

            var boolColor = this.Component.Value ? Color.FromArgb(39, 96, 17) : Color.FromArgb(85, 25, 15);

            RenderManager.RenderRectangle(
                indBoxPosition,
                DefaultMenuTheme.IndicatorWidth,
                this.Theme.MenuHeight - (DefaultMenuTheme.LineWidth),
                boolColor);

            var centerArrowBox = indBoxPosition + new Vector2(DefaultMenuTheme.IndicatorWidth / 2, this.Theme.MenuHeight / 2);

            RenderManager.RenderText(
                centerArrowBox,
                Color.AliceBlue,
                this.Component.Value ? "ON" : "OFF", RenderTextFlags.HorizontalCenter | RenderTextFlags.VerticalCenter);
        }

        #endregion
    }
}