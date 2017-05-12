namespace Aimtec.SDK.Menu.Theme.Default
{
    using System;
    using System.Drawing;

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
            this.Theme.DrawMenuItemBorder(pos);

            var position = pos + DefaultMenuTheme.LineWidth;

            this.Theme.DrawMenuItemBox(position);

            var displayString = this.Component.DisplayName;
            
            // Todo measure text
            RenderManager.RenderText(
                pos
                + new Vector2(
                    DefaultMenuTheme.TextSpacing,
                    (this.Theme.MenuHeight - DefaultMenuTheme.LineWidth) / 4f + 9 / 2f - 1),
                // not bro science
                Color.FromArgb(207, 195, 149),
                this.Component.DisplayName);


            // Render arrow outline
            RenderManager.RenderLine(
                pos.X + this.Theme.ComponentWidth - DefaultMenuTheme.ArrowWidth - DefaultMenuTheme.LineWidth,
                pos.Y,
                pos.X + this.Theme.ComponentWidth - DefaultMenuTheme.ArrowWidth - DefaultMenuTheme.LineWidth,
                pos.Y + this.Theme.MenuHeight,
                Color.FromArgb(82, 83, 57));

            //Draw Key inidcator
            RenderManager.RenderText(
               pos
               + new Vector2(
                   this.Theme.ComponentWidth - DefaultMenuTheme.ArrowWidth - DefaultMenuTheme.LineWidth - (this.Component.KeyIsBeingSet ? 85 : 30), //todo: measure text rather than explicit values
                   (this.Theme.MenuHeight - DefaultMenuTheme.LineWidth) / 4f + 9 / 2f - 1),
               // not bro science
               Color.FromArgb(207, 195, 149),
               this.Component.KeyIsBeingSet ? "PRESS KEY" : "[" + (uint)this.Component.Key + "]");

            // Draw arrow box
            position += new Vector2(this.Theme.ComponentWidth - DefaultMenuTheme.ArrowWidth - DefaultMenuTheme.LineWidth, 0);

            var boolColor = this.Component.Value ? Color.FromArgb(39, 96, 17) : Color.FromArgb(85, 25, 15);

            RenderManager.RenderRectangle(
                position,
                DefaultMenuTheme.ArrowWidth,
                this.Theme.MenuHeight - DefaultMenuTheme.LineWidth,
                boolColor);

            // todo center text in box
            RenderManager.RenderText(
                position + new Vector2(5, 10),
                Color.AliceBlue,
                this.Component.Value ? "ON" : "OFF");
        }

        #endregion
    }
}