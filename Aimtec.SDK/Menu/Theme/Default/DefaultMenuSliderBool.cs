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
                / (this.Component.MaxValue - this.Component.MinValue) * this.Theme.ComponentWidth;

            var afterSliderWidth = this.Theme.ComponentWidth - DefaultMenuTheme.IndicatorWidth  - beforeSliderWidth;

            this.Theme.DrawMenuItemBorder(pos);

            var position = pos + DefaultMenuTheme.LineWidth;

            this.Theme.DrawMenuItemBox(position);

            // Draw light bar before the slider line
            RenderManager.RenderRectangle(
                position,
                beforeSliderWidth,
                this.Theme.MenuHeight * 0.95f,
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

            var position2 = position + new Vector2(beforeSliderWidth, 0);

            RenderManager.RenderLine(
                position2,
                position2 + new Vector2(0, this.Theme.MenuHeight),
                DefaultMenuTheme.LineWidth,
                false,
                Color.FromArgb(82, 83, 57));

            var position3 = position2 + new Vector2(DefaultMenuTheme.LineWidth, 0);

            RenderManager.RenderRectangle(
                position3,
                afterSliderWidth - DefaultMenuTheme.LineWidth * 2,
                this.Theme.MenuHeight * 0.95f,
                Color.FromArgb(16, 26, 29));

            // draw text
            RenderManager.RenderText(
                pos + DefaultMenuTheme.LineWidth
                + new Vector2(
                    this.Theme.ComponentWidth - DefaultMenuTheme.IndicatorWidth - 25,
                    (this.Theme.MenuHeight - DefaultMenuTheme.LineWidth) / 4f + 9 / 2f - 1),
                Color.FromArgb(207, 195, 149),
                this.Component.Value.ToString());


            // Render arrow outline
            RenderManager.RenderLine(
                pos.X + this.Theme.ComponentWidth - DefaultMenuTheme.IndicatorWidth  - DefaultMenuTheme.LineWidth,
                pos.Y,
                pos.X + this.Theme.ComponentWidth - DefaultMenuTheme.IndicatorWidth  - DefaultMenuTheme.LineWidth,
                pos.Y + this.Theme.MenuHeight,
                Color.FromArgb(82, 83, 57));

            // Draw arrow box
            var positionArrowBox = position + new Vector2(this.Theme.ComponentWidth - DefaultMenuTheme.IndicatorWidth  - DefaultMenuTheme.LineWidth, 0);

            var boolColor = this.Component.Enabled ? Color.FromArgb(39, 96, 17) : Color.FromArgb(85, 25, 15);

            RenderManager.RenderRectangle(
                positionArrowBox,
                DefaultMenuTheme.IndicatorWidth,
                this.Theme.MenuHeight - DefaultMenuTheme.LineWidth,
                boolColor);

            // todo center text in box
            RenderManager.RenderText(
                positionArrowBox + new Vector2(5, 10),
                Color.AliceBlue,
                this.Component.Enabled ? "ON" : "OFF");
        }

        #endregion
    }
}