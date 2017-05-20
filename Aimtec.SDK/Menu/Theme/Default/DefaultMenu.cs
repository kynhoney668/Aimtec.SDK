namespace Aimtec.SDK.Menu.Theme.Default
{
    using System.Drawing;

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
            this.Theme.DrawMenuItemBorder(pos, this.Component.Root);
   
            var position = pos + DefaultMenuTheme.LineWidth;

            this.Theme.DrawMenuItemBox(position, this.Component.Root);

            var displayNamePosition = position + new Vector2(DefaultMenuTheme.TextSpacing, (this.Theme.MenuHeight) / 2);

            RenderManager.RenderText(
                displayNamePosition,
                this.Theme.TextColor,
                this.Component.DisplayName, RenderTextFlags.VerticalCenter | RenderTextFlags.HorizontalLeft);

            var width = this.Component.Root ? this.Theme.RootMenuWidth : this.Theme.ComponentWidth;

            // Render arrow outline
            RenderManager.RenderLine(
                pos.X + width - DefaultMenuTheme.IndicatorWidth - DefaultMenuTheme.LineWidth,
                pos.Y,
                pos.X + width - DefaultMenuTheme.IndicatorWidth - DefaultMenuTheme.LineWidth,
                pos.Y + this.Theme.MenuHeight,
                this.Theme.LineColor);

            // Draw arrow box
            position += new Vector2(width - DefaultMenuTheme.IndicatorWidth - DefaultMenuTheme.LineWidth, 0);

            var arrowBoxColor = Color.FromArgb(14, 59, 73);

            if (!this.Component.Toggled)
            {
                arrowBoxColor = Color.FromArgb(11, 18, 20);
            }

            RenderManager.RenderRectangle(
                position,
                DefaultMenuTheme.IndicatorWidth ,
                this.Theme.MenuHeight - DefaultMenuTheme.LineWidth,
                arrowBoxColor);


            RenderManager.RenderText(position + new Vector2((DefaultMenuTheme.IndicatorWidth  / 2), (this.Theme.MenuHeight - DefaultMenuTheme.LineWidth) / 2), Color.FromArgb(207, 195, 149), ">", RenderTextFlags.VerticalCenter | RenderTextFlags.HorizontalCenter);

        }

        #endregion
    }
}