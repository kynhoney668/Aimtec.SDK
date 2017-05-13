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
            this.Theme.DrawMenuItemBorder(pos);

            var position = pos + DefaultMenuTheme.LineWidth;

            this.Theme.DrawMenuItemBoxFull(position);

            RenderManager.RenderText(
                position + new Vector2(this.Theme.ComponentWidth / 2f - 50, this.Theme.MenuHeight / 2f - 5),
                Color.FromArgb(207, 195, 149),
                this.Component.Value);
        }

        #endregion
    }
}