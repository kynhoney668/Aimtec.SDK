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
            var beforeSliderWidth = (float)(this.Component.Value - this.Component.MinValue)
                / (this.Component.MaxValue - this.Component.MinValue) * (this.Theme.ComponentWidth - DefaultMenuTheme.IndicatorWidth - DefaultMenuTheme.LineWidth * 2);

            var afterSliderWidth = this.Theme.ComponentWidth - DefaultMenuTheme.IndicatorWidth - beforeSliderWidth - DefaultMenuTheme.LineWidth;

            this.Theme.DrawMenuItemBorder(pos);

            var position = pos + DefaultMenuTheme.LineWidth;

            this.Theme.DrawMenuItemBox(position);

            var displayNamePosition = position + new Vector2(DefaultMenuTheme.TextSpacing, (this.Theme.MenuHeight) / 2);

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

            var beforeSliderPos = position + new Vector2(beforeSliderWidth, 0);

            RenderManager.RenderLine(
                beforeSliderPos,
                beforeSliderPos + new Vector2(0, this.Theme.MenuHeight),
                DefaultMenuTheme.LineWidth,
                false,
                Color.FromArgb(82, 83, 57));

            var afterSliderPos = beforeSliderPos + new Vector2(DefaultMenuTheme.LineWidth, 0);

            RenderManager.RenderRectangle(
                afterSliderPos,
                afterSliderWidth - DefaultMenuTheme.LineWidth * 2,
                this.Theme.MenuHeight * 0.95f,
                Color.FromArgb(16, 26, 29));

            // draw text
            RenderManager.RenderText(
                pos + DefaultMenuTheme.LineWidth
                + new Vector2(
                    this.Theme.ComponentWidth - DefaultMenuTheme.IndicatorWidth - DefaultMenuTheme.TextSpacing,
                    this.Theme.MenuHeight / 2),
                Color.FromArgb(207, 195, 149),
                this.Component.Value.ToString(), RenderTextFlags.HorizontalRight | RenderTextFlags.VerticalCenter);


            // Render indicator box outline
            RenderManager.RenderLine(
                pos.X + this.Theme.ComponentWidth - DefaultMenuTheme.IndicatorWidth  - DefaultMenuTheme.LineWidth,
                pos.Y,
                pos.X + this.Theme.ComponentWidth - DefaultMenuTheme.IndicatorWidth  - DefaultMenuTheme.LineWidth,
                pos.Y + this.Theme.MenuHeight,
                Color.FromArgb(82, 83, 57));

            // Draw indicator box
            var boolColor = this.Component.Enabled ? Color.FromArgb(39, 96, 17) : Color.FromArgb(85, 25, 15);

            var indBoxPosition = position + new Vector2(this.Theme.ComponentWidth - DefaultMenuTheme.IndicatorWidth - DefaultMenuTheme.LineWidth, 0);

            RenderManager.RenderRectangle(
                indBoxPosition,
                DefaultMenuTheme.IndicatorWidth,
                this.Theme.MenuHeight - DefaultMenuTheme.LineWidth,
                boolColor);

            var centerArrowBox = indBoxPosition + new Vector2(DefaultMenuTheme.IndicatorWidth / 2, this.Theme.MenuHeight / 2);

            RenderManager.RenderText(
                centerArrowBox,
                Color.AliceBlue,
                this.Component.Enabled ? "ON" : "OFF", RenderTextFlags.HorizontalCenter | RenderTextFlags.VerticalCenter);
        }

        #endregion
    }
}