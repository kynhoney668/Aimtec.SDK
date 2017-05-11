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
                / (this.Component.MaxValue - this.Component.MinValue) * this.Theme.MenuWidth;

            var afterSliderWidth = this.Theme.MenuWidth - beforeSliderWidth;

            this.Theme.DrawMenuItemBorder(pos);

            var position = pos + DefaultMenuTheme.LineWidth;

            // Draw light bar before the slider line
            RenderManager.RenderRectangle(
                position,
                beforeSliderWidth,
                this.Theme.MenuHeight,
                Color.FromArgb(14, 59, 73));

            // Todo measure text
            RenderManager.RenderText(
                pos
                + new Vector2(
                    DefaultMenuTheme.TextSpacing,
                    (this.Theme.MenuHeight - DefaultMenuTheme.LineWidth) / 4f + 9 / 2f - 1),
                // not bro science
                Color.FromArgb(207, 195, 149),
                this.Component.DisplayName);

            position += new Vector2(beforeSliderWidth, 0);

            RenderManager.RenderLine(
                position,
                position + new Vector2(0, this.Theme.MenuHeight),
                DefaultMenuTheme.LineWidth,
                false,
                Color.FromArgb(82, 83, 57));

            position += new Vector2(DefaultMenuTheme.LineWidth, 0);

            RenderManager.RenderRectangle(
                position,
                afterSliderWidth - DefaultMenuTheme.LineWidth * 2,
                this.Theme.MenuHeight,
                Color.FromArgb(16, 26, 29));

            // draw text
            RenderManager.RenderText(
                pos + DefaultMenuTheme.LineWidth
                + new Vector2(
                    this.Theme.MenuWidth - 25,
                    (this.Theme.MenuHeight - DefaultMenuTheme.LineWidth) / 4f + 9 / 2f - 1),
                Color.FromArgb(207, 195, 149),
                this.Component.Value.ToString());
        }

        #endregion
    }
}