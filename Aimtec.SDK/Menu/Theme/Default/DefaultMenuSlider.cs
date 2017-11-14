namespace Aimtec.SDK.Menu.Theme.Default
{
    using System.Drawing;

    using Aimtec.SDK.Menu.Components;

    internal class DefaultMenuSlider : IRenderManager<MenuSlider, DefaultMenuTheme>
    {
        #region Constructors and Destructors

        public DefaultMenuSlider(MenuSlider slider, DefaultMenuTheme defaultMenuTheme)
        {
            this.Component = slider;
            this.Theme = defaultMenuTheme;
        }

        #endregion

        #region Public Properties

        public MenuSlider Component { get; }

        public DefaultMenuTheme Theme { get; }

        #endregion

        #region Public Methods and Operators

        public void Render(Vector2 pos)
        {
            var width = this.Component.Parent.Width;
            var height = MenuManager.MaxHeightItem + this.Theme.BonusMenuHeight;

            var beforeSliderWidth = (float) (this.Component.Value - this.Component.MinValue)
                / (this.Component.MaxValue - this.Component.MinValue) * (width - this.Theme.LineWidth * 2);

            var afterSliderWidth = width - beforeSliderWidth - this.Theme.LineWidth;

            this.Theme.DrawMenuItemBorder(pos, width);

            var position = pos + this.Theme.LineWidth;

            this.Theme.DrawMenuItemBox(position, width);

            var displayNamePosition = new Aimtec.Rectangle((int)position.X + this.Theme.TextSpacing, (int)position.Y, (int)(position.X + width), (int)(position.Y + height));

            // Draw light bar before the slider line
            Aimtec.Render.Rectangle(
                position,
                beforeSliderWidth,
                height * 0.95f,
                Color.FromArgb(14, 59, 73));

            var bfSlider = position + new Vector2(beforeSliderWidth, 0);

            Aimtec.Render.Line(
                bfSlider,
                bfSlider + new Vector2(0, height),
                this.Theme.LineWidth,
                false,
                Color.FromArgb(82, 83, 57));

            var afSlider = bfSlider;

            Aimtec.Render.Rectangle(
                afSlider,
                afterSliderWidth - this.Theme.LineWidth,
                height * 0.95f,
                Color.FromArgb(16, 26, 29));

            // draw text
            FontManager.CurrentFont.Draw(this.Component.Value.ToString(),
                new Aimtec.Rectangle((int)pos.X + this.Theme.LineWidth, (int)pos.Y + this.Theme.LineWidth, (int)(pos.X + width - this.Theme.TextSpacing), (int)(pos.Y + height)),
                RenderTextFlags.VerticalCenter | RenderTextFlags.HorizontalRight, this.Theme.TextColor);

            FontManager.CurrentFont.Draw(this.Component.DisplayName + (!string.IsNullOrEmpty(this.Component.ToolTip) ? " [?]" : ""),
                displayNamePosition,
                RenderTextFlags.VerticalCenter, this.Theme.TextColor);
        }

        #endregion
    }
}