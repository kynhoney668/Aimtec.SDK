namespace Aimtec.SDK.Menu.Theme
{
    using Aimtec.SDK.Menu.Components;
    using System.Drawing;

    /// <summary>
    ///     Class MenuTheme.
    /// </summary>
    public abstract class MenuTheme
    {
        #region Public Properties

        /// <summary>
        ///     Gets the height of the menu.
        /// </summary>
        /// <value>The height of the menu.</value>
        public abstract int MenuHeight { get; }

        /// <summary>
        ///     Gets the width of the menu.
        /// </summary>
        /// <value>The width of the root menu items.</value>
        public abstract int RootMenuWidth { get; set; }

        /// <summary>
        ///     Gets the width of the menu components.
        /// </summary>
        /// <value>The width of the menu.</value>
        public abstract int ComponentWidth { get; set; }


        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Builds the menu bool renderer.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <returns>IRenderManager&lt;MenuBool&gt;.</returns>
        public abstract IRenderManager<MenuBool> BuildMenuBoolRenderer(MenuBool component);

        /// <summary>
        ///     Builds the menu keybind renderer.
        /// </summary>
        /// <param name="keybind">The keybind.</param>
        /// <returns>IRenderManager&lt;MenuKeybind&gt;.</returns>
        public abstract IRenderManager<MenuKeyBind> BuildMenuKeyBindRenderer(MenuKeyBind keybind);

        /// <summary>
        ///     Builds the menu list.
        /// </summary>
        /// <param name="menuList">The menu list.</param>
        /// <returns>IRenderManager&lt;MenuList&gt;.</returns>
        public abstract IRenderManager<MenuList> BuildMenuList(MenuList menuList);

        /// <summary>
        ///     Builds the menu slider bool renderer.
        /// </summary>
        /// <param name="menuList">The menu list.</param>
        /// <returns>IRenderManager&lt;MenuList&gt;.</returns>
        public abstract IRenderManager<MenuSliderBool> BuildMenuSliderBoolRenderer(MenuSliderBool menuSliderBool);

        /// <summary>
        ///     Builds the menu renderer.
        /// </summary>
        /// <param name="menu">The menu.</param>
        /// <returns>IRenderManager&lt;IMenu&gt;.</returns>
        public abstract IRenderManager<IMenu> BuildMenuRenderer(IMenu menu);

        /// <summary>
        ///     Builds the menu seperator renderer.
        /// </summary>
        /// <param name="menuSeperator">The menu seperator.</param>
        /// <returns>IRenderManager&lt;MenuSeperator&gt;.</returns>
        public abstract IRenderManager<MenuSeperator> BuildMenuSeperatorRenderer(MenuSeperator menuSeperator);

        /// <summary>
        ///     Builds the menu slider renderer.
        /// </summary>
        /// <param name="slider">The slider.</param>
        /// <returns>IRenderManager&lt;MenuSlider&gt;.</returns>
        public abstract IRenderManager<MenuSlider> BuildMenuSliderRenderer(MenuSlider slider);

        public abstract Rectangle GetMenuBoolControlBounds(Vector2 pos);

        public abstract Rectangle GetMenuSliderControlBounds(Vector2 pos);

        public abstract Rectangle[] GetMenuListControlBounds(Vector2 pos);

        public abstract Rectangle[] GetMenuSliderBoolControlBounds(Vector2 pos);


        #endregion
    }
}