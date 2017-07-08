namespace Aimtec.SDK.Menu.Theme.Default
{
    using System.Drawing;

    using Aimtec.SDK.Menu.Components;

    internal class DefaultMenuSliderBool : IRenderManager<MenuSliderBool, DefaultMenuTheme>
    {
        #region Constructors and Destructors

        public DefaultMenuSliderBool(MenuSliderBool sliderBool, DefaultMenuTheme defaultMenuTheme)
        {
            this.Component = sliderBool;
            this.Theme = defaultMenuTheme;
        }

        #endregion

        #region Public Properties

        public MenuSliderBool Component { get; }

        public DefaultMenuTheme Theme { get; }

        #endregion

        #region Public Methods and Operators

        public void Render(Vector2 pos)
        {
            var width = this.Component.Parent.Width;

            var beforeSliderWidth = (float)(this.Component.Value - this.Component.MinValue)
                / (this.Component.MaxValue - this.Component.MinValue) * (width - DefaultMenuTheme.IndicatorWidth - DefaultMenuTheme.LineWidth * 2);

            var afterSliderWidth = width - DefaultMenuTheme.IndicatorWidth - beforeSliderWidth - DefaultMenuTheme.LineWidth;

            this.Theme.DrawMenuItemBorder(pos, width);

            var position = pos + DefaultMenuTheme.LineWidth;

            this.Theme.DrawMenuItemBox(position, width);

            var displayNamePosition = position + new Vector2(DefaultMenuTheme.TextSpacing, (this.Theme.MenuHeight) / 2);

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

            var beforeSliderPos = position + new Vector2(beforeSliderWidth, 0);

            Aimtec.Render.Line(
                beforeSliderPos,
                beforeSliderPos + new Vector2(0, this.Theme.MenuHeight),
                DefaultMenuTheme.LineWidth,
                false,
                Color.FromArgb(82, 83, 57));

            var afterSliderPos = beforeSliderPos + new Vector2(DefaultMenuTheme.LineWidth, 0);

            Aimtec.Render.Rectangle(
                afterSliderPos,
                afterSliderWidth - DefaultMenuTheme.LineWidth * 2,
                this.Theme.MenuHeight * 0.95f,
                Color.FromArgb(16, 26, 29));

            // draw text
            Aimtec.Render.Text(
                pos + DefaultMenuTheme.LineWidth
                + new Vector2(
                    width - DefaultMenuTheme.IndicatorWidth - DefaultMenuTheme.TextSpacing,
                    this.Theme.MenuHeight / 2),
                Color.FromArgb(207, 195, 149),
                this.Component.Value.ToString(), RenderTextFlags.HorizontalRight | RenderTextFlags.VerticalCenter);


            // Render indicator box outline
            Aimtec.Render.Line(
                pos.X + width - DefaultMenuTheme.IndicatorWidth  - DefaultMenuTheme.LineWidth,
                pos.Y,
                pos.X + width - DefaultMenuTheme.IndicatorWidth  - DefaultMenuTheme.LineWidth,
                pos.Y + this.Theme.MenuHeight,
                Color.FromArgb(82, 83, 57));

            // Draw indicator box
            var boolColor = this.Component.Enabled ? Color.FromArgb(39, 96, 17) : Color.FromArgb(85, 25, 15);

            var indBoxPosition = position + new Vector2(width - DefaultMenuTheme.IndicatorWidth - DefaultMenuTheme.LineWidth, 0);

            Aimtec.Render.Rectangle(
                indBoxPosition,
                DefaultMenuTheme.IndicatorWidth,
                this.Theme.MenuHeight - DefaultMenuTheme.LineWidth,
                boolColor);

            var centerArrowBox = indBoxPosition + new Vector2(DefaultMenuTheme.IndicatorWidth / 2, this.Theme.MenuHeight / 2);

            Aimtec.Render.Text(
                centerArrowBox,
                Color.AliceBlue,
                this.Component.Enabled ? "ON" : "OFF", RenderTextFlags.HorizontalCenter | RenderTextFlags.VerticalCenter);
        }

        #endregion
    }
}