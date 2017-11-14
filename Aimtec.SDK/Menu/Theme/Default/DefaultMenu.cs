namespace Aimtec.SDK.Menu.Theme.Default
{
    using System;
    using System.Drawing;

    using Rectangle = Aimtec.Rectangle;

    internal class DefaultMenu : IRenderManager<Menu, DefaultMenuTheme>
    {
        #region Constructors and Destructors

        public DefaultMenu(Menu component, DefaultMenuTheme theme)
        {
            this.Component = component;
            this.Theme = theme;
        }

        #endregion

        #region Public Properties

        public Menu Component { get; }

        public DefaultMenuTheme Theme { get; }

        #endregion

        #region Public Methods and Operators

        public void Render(Vector2 pos)
        {
            var width = this.Component.Parent.Width;

            var height = MenuManager.MaxHeightItem + this.Theme.BonusMenuHeight;

            this.Theme.DrawMenuItemBorder(pos, width);

            var position = pos + new Vector2(this.Theme.LineWidth, this.Theme.LineWidth);

            this.Theme.DrawMenuItemBox(position, width);

            var leftVCenter = RenderTextFlags.VerticalCenter | RenderTextFlags.HorizontalLeft | RenderTextFlags.NoClip
                        | RenderTextFlags.SingleLine;

            var center = RenderTextFlags.VerticalCenter | RenderTextFlags.HorizontalCenter | RenderTextFlags.NoClip
                         | RenderTextFlags.SingleLine;

            var displayNamePosition = new Rectangle((int)position.X + this.Theme.TextSpacing, (int)position.Y, (int) (position.X + width), (int) (position.Y + height));

            FontManager.CurrentFont.Draw(this.Component.DisplayName,
                displayNamePosition,
                leftVCenter, this.Theme.TextColor);

            // Render arrow outline
            Aimtec.Render.Line(
                pos.X + width - this.Theme.IndicatorWidth - this.Theme.LineWidth,
                pos.Y,
                pos.X + width - this.Theme.IndicatorWidth - this.Theme.LineWidth,
                pos.Y + height,
                this.Theme.LineColor);

            // Draw arrow box
            position += new Vector2(width - this.Theme.IndicatorWidth - this.Theme.LineWidth, 0);

            var arrowBoxColor = Color.FromArgb(14, 59, 73);

            if (!this.Component.Toggled)
            {
                arrowBoxColor = Color.FromArgb(11, 18, 20);
            }

            Aimtec.Render.Rectangle(
                position,
                this.Theme.IndicatorWidth,
                height - this.Theme.LineWidth,
                arrowBoxColor);

            FontManager.CurrentFont.Draw(">",new Aimtec.Rectangle((int)position.X, (int)position.Y, (int) (position.X + this.Theme.IndicatorWidth), (int)(position.Y + (height - this.Theme.LineWidth))), center, this.Theme.TextColor);

        }

        #endregion
    }
}