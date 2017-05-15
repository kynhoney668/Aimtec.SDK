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
            var beforeSliderWidth = (float) (this.Component.Value - this.Component.MinValue)
                / (this.Component.MaxValue - this.Component.MinValue) * (this.Theme.ComponentWidth);

            var afterSliderWidth = this.Theme.ComponentWidth - beforeSliderWidth;

            this.Theme.DrawMenuItemBorder(pos);

            var position = pos + DefaultMenuTheme.LineWidth;

            this.Theme.DrawMenuItemBox(position);

            var displayNamePosition = position + new Vector2(DefaultMenuTheme.TextSpacing, (this.Theme.MenuHeight) / 2f);

            // Draw light bar before the slider line
            RenderManager.RenderRectangle(
                position,
                beforeSliderWidth,
                this.Theme.MenuHeight * 0.95f,
                Color.FromArgb(14, 59, 73));

            RenderManager.RenderText(
                displayNamePosition,
                Color.FromArgb(207, 195, 149),
                this.Component.DisplayName, RenderTextFlags.VerticalCenter);

            var bfSlider = position + new Vector2(beforeSliderWidth, 0);

            RenderManager.RenderLine(
                bfSlider,
                bfSlider + new Vector2(0, this.Theme.MenuHeight),
                DefaultMenuTheme.LineWidth,
                false,
                Color.FromArgb(82, 83, 57));

            var afSlider = bfSlider + new Vector2(DefaultMenuTheme.LineWidth, 0);

            RenderManager.RenderRectangle(
                afSlider,
                afterSliderWidth - DefaultMenuTheme.LineWidth * 2,
                this.Theme.MenuHeight * 0.95f,
                Color.FromArgb(16, 26, 29));

            // draw text
            RenderManager.RenderText(
                pos + DefaultMenuTheme.LineWidth
                + new Vector2(
                    this.Theme.ComponentWidth - DefaultMenuTheme.TextSpacing,
                    (this.Theme.MenuHeight) / 2f),
                Color.FromArgb(207, 195, 149),
                this.Component.Value.ToString(), RenderTextFlags.VerticalCenter | RenderTextFlags.HorizontalRight);
        }

        #endregion
    }
}