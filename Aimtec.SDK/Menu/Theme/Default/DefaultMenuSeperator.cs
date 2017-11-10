namespace Aimtec.SDK.Menu.Theme.Default
{
    using System.Drawing;

    using Aimtec.SDK.Menu.Components;

    internal class DefaultMenuSeperator : IRenderManager<MenuSeperator, DefaultMenuTheme>
    {
        #region Constructors and Destructors

        public DefaultMenuSeperator(MenuSeperator component, DefaultMenuTheme theme)
        {
            this.Component = component;
            this.Theme = theme;
        }

        #endregion

        #region Public Properties

        public MenuSeperator Component { get; }

        public DefaultMenuTheme Theme { get; }

        #endregion

        #region Public Methods and Operators

        public void Render(Vector2 pos)
        {
            var width = this.Component.Parent.Width;

            var height = MenuManager.MaxHeightItem + this.Theme.BonusMenuHeight;

            this.Theme.DrawMenuItemBorder(pos, width);

            var position = pos + this.Theme.LineWidth;

            this.Theme.DrawMenuItemBoxFull(position, width);

            var center = RenderTextFlags.VerticalCenter | RenderTextFlags.HorizontalCenter | RenderTextFlags.NoClip
                         | RenderTextFlags.SingleLine;

            var textPosition = new Aimtec.Rectangle((int)position.X, (int)position.Y, (int)(position.X + width), (int)(position.Y + height));

            FontManager.CurrentFont.Draw(this.Component.Value,
                textPosition,
                center, this.Theme.TextColor);

        }

        #endregion
    }
}