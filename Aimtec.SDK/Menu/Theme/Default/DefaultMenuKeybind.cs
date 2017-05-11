namespace Aimtec.SDK.Menu.Theme.Default
{
    using System;

    using Aimtec.SDK.Menu.Components;

    internal class DefaultMenuKeybind : IRenderManager<MenuKeybind, DefaultMenuTheme>
    {
        #region Constructors and Destructors

        public DefaultMenuKeybind(MenuKeybind component, DefaultMenuTheme theme)
        {
            this.Component = component;
            this.Theme = theme;
        }

        #endregion

        #region Public Properties

        public MenuKeybind Component { get; }

        public DefaultMenuTheme Theme { get; }

        #endregion

        #region Public Methods and Operators

        public void Render(Vector2 pos)
        {
            // todo
            throw new NotImplementedException();
        }

        #endregion
    }
}