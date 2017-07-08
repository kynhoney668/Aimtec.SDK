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

            var beforeSliderWidth = (float) (this.Component.Value - this.Component.MinValue)
                / (this.Component.MaxValue - this.Component.MinValue) * (width - DefaultMenuTheme.LineWidth * 2);

            var afterSliderWidth = width - beforeSliderWidth - DefaultMenuTheme.LineWidth;

            this.Theme.DrawMenuItemBorder(pos, width);

            var position = pos + DefaultMenuTheme.LineWidth;

            this.Theme.DrawMenuItemBox(position, width);

            var displayNamePosition = position + new Vector2(DefaultMenuTheme.TextSpacing, (this.Theme.MenuHeight) / 2f);

            // Draw light bar before the slider line
            Aimtec.Render.Rectangle(
                position,
                beforeSliderWidth,
                this.Theme.MenuHeight * 0.95f,
                Color.FromArgb(14, 59, 73));

            Aimtec.Render.Text(
                displayNamePosition,
                Color.FromArgb(207, 195, 149),
                this.Component.DisplayName, RenderTextFlags.VerticalCenter);

            var bfSlider = position + new Vector2(beforeSliderWidth, 0);

            Aimtec.Render.Line(
                bfSlider,
                bfSlider + new Vector2(0, this.Theme.MenuHeight),
                DefaultMenuTheme.LineWidth,
                false,
                Color.FromArgb(82, 83, 57));

            var afSlider = bfSlider;

            Aimtec.Render.Rectangle(
                afSlider,
                afterSliderWidth - DefaultMenuTheme.LineWidth,
                this.Theme.MenuHeight * 0.95f,
                Color.FromArgb(16, 26, 29));

            // draw text
            Aimtec.Render.Text(
                pos + DefaultMenuTheme.LineWidth
                + new Vector2(
                    width - DefaultMenuTheme.TextSpacing,
                    (this.Theme.MenuHeight) / 2f),
                Color.FromArgb(207, 195, 149),
                this.Component.Value.ToString(), RenderTextFlags.VerticalCenter | RenderTextFlags.HorizontalRight);
        }

        #endregion
    }
}