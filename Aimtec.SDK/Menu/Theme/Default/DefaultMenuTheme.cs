namespace Aimtec.SDK.Menu.Theme.Default
{
    using System;
    using System.Drawing;

    using Aimtec.SDK.Menu.Components;

    public class DefaultMenuTheme : MenuTheme
    {
        #region Public Properties

        public static int ArrowWidth { get; } = 35;

        public static int BoolIndicatorWidth { get; } = 35;

        public static int LineWidth { get; } = 1;

        public static int TextSpacing { get; } = 15;

        public Color LineColor { get; } = Color.FromArgb(82, 83, 57);

        public Color MenuBoxBackgroundColor { get; } = Color.FromArgb(206, 16, 26, 29);

        public override int MenuHeight { get; } = 32;

        public override int MenuWidth { get; } = 160;

        public override int ComponentWidth { get; } = 250;

        public Color TextColor { get; } = Color.FromArgb(207, 195, 149);

        #endregion

        #region Public Methods and Operators

        public static void DrawRectangleOutline(
            float x,
            float y,
            float width,
            float height,
            float lineWidth,
            Color color)
        {
            // Top left to top right
            RenderManager.RenderLine(x, y, x + width, y, lineWidth, true, color);

            // Top right to bottom right
            RenderManager.RenderLine(x + width, y, x + width, y + height, lineWidth, true, color);

            // Bottom right to bottom left
            RenderManager.RenderLine(x + width, y + height, x, y + height, lineWidth, true, color);

            // Bottom left to top left
            RenderManager.RenderLine(x, y + height, x, y, lineWidth, true, color);
        }

        public override IRenderManager<MenuBool> BuildMenuBoolRenderer(MenuBool component)
        {
            return new DefaultMenuBool(component, this);
        }

        public override IRenderManager<MenuKeybind> BuildMenuKeybindRenderer(MenuKeybind keybind)
        {
            return new DefaultMenuKeybind(keybind, this);
        }

        public override IRenderManager<MenuList> BuildMenuList(MenuList menuList)
        {
            return new DefaultMenuList(menuList, this);
        }

        public override IRenderManager<IMenu> BuildMenuRenderer(IMenu menu)
        {
            return new DefaultMenu(menu, this);
        }

        public override IRenderManager<MenuSeperator> BuildMenuSeperatorRenderer(MenuSeperator menuSeperator)
        {
            return new DefaultMenuSeperator(menuSeperator, this);
        }

        public override IRenderManager<MenuSlider> BuildMenuSliderRenderer(MenuSlider slider)
        {
            return new DefaultMenuSlider(slider, this);
        }

        #endregion

        #region Methods

        internal void DrawMenuBox(Vector2 position)
        {
            RenderManager.RenderRectangle(
                position,
                this.MenuWidth - ArrowWidth - LineWidth,
                this.MenuHeight - LineWidth,
                this.MenuBoxBackgroundColor);
        }

        internal void DrawMenuItemBox(Vector2 position)
        {
            RenderManager.RenderRectangle(
                position,
                this.ComponentWidth - ArrowWidth - LineWidth,
                this.MenuHeight - LineWidth,
                this.MenuBoxBackgroundColor);
        }

        internal void DrawMenuBorder(Vector2 pos)
        {
            DrawRectangleOutline(pos.X, pos.Y, this.MenuWidth, this.MenuHeight, LineWidth, this.LineColor);
        }

        internal void DrawMenuItemBorder(Vector2 pos)
        {
            DrawRectangleOutline(pos.X, pos.Y, this.ComponentWidth, this.MenuHeight, LineWidth, this.LineColor);
        }

        public override Rectangle GetControlObjectBounds(Vector2 pos, MenuItemType type)
        {
            var newPos = pos + new Vector2(this.ComponentWidth - ArrowWidth - LineWidth, 0);
            return new Rectangle(
              (int)newPos.X,
              (int)newPos.Y,
              BoolIndicatorWidth,
              MenuHeight);
        }



        #endregion
    }
}