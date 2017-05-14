namespace Aimtec.SDK.Menu.Theme.Default
{
    using System;
    using System.Drawing;

    using Aimtec.SDK.Menu.Components;
    using Util;

    internal class DefaultMenuKeyBind : IRenderManager<MenuKeyBind, DefaultMenuTheme>
    {
        #region Constructors and Destructors

        public DefaultMenuKeyBind(MenuKeyBind component, DefaultMenuTheme theme)
        {
            this.Component = component;
            this.Theme = theme;
        }

        #endregion

        #region Public Properties

        public MenuKeyBind Component { get; }

        public DefaultMenuTheme Theme { get; }

        #endregion

        #region Public Methods and Operators

        public void Render(Vector2 pos)
        {
            this.Theme.DrawMenuItemBorder(pos);

            var position = pos + DefaultMenuTheme.LineWidth;

            this.Theme.DrawMenuItemBox(position);

            var displayNamePosition = position + new Vector2(DefaultMenuTheme.TextSpacing, (this.Theme.MenuHeight) / 2);

            RenderManager.RenderText(
                displayNamePosition,
                Color.FromArgb(207, 195, 149),
                this.Component.DisplayName, RenderTextFlags.VerticalCenter);

            // Render indicator box outline
            RenderManager.RenderLine(
                pos.X + this.Theme.ComponentWidth - DefaultMenuTheme.IndicatorWidth - DefaultMenuTheme.LineWidth,
                pos.Y,
                pos.X + this.Theme.ComponentWidth - DefaultMenuTheme.IndicatorWidth  - DefaultMenuTheme.LineWidth,
                pos.Y + this.Theme.MenuHeight,
                Color.FromArgb(82, 83, 57));

            // Draw indicator box
            var indBoxPosition = position + new Vector2(this.Theme.ComponentWidth - DefaultMenuTheme.IndicatorWidth - DefaultMenuTheme.LineWidth, 0);

            var boolColor = this.Component.Value ? Color.FromArgb(39, 96, 17) : Color.FromArgb(85, 25, 15);

            RenderManager.RenderRectangle(
                indBoxPosition,
                DefaultMenuTheme.IndicatorWidth ,
                this.Theme.MenuHeight - DefaultMenuTheme.LineWidth,
                boolColor);

            var centerArrowBox = indBoxPosition + new Vector2(DefaultMenuTheme.IndicatorWidth / 2, this.Theme.MenuHeight / 2);

            RenderManager.RenderText(
                centerArrowBox,
                Color.AliceBlue,
                this.Component.Value ? "ON" : "OFF", RenderTextFlags.HorizontalCenter | RenderTextFlags.VerticalCenter);

            //Draw Key indicator
            var keyIndicatorPos = pos + new Vector2(this.Theme.ComponentWidth - DefaultMenuTheme.IndicatorWidth - DefaultMenuTheme.LineWidth - DefaultMenuTheme.TextSpacing, this.Theme.MenuHeight / 2);

            RenderManager.RenderText(
              keyIndicatorPos,
               Color.FromArgb(207, 195, 149),
               this.Component.KeyIsBeingSet ? "PRESS KEY" : "[" + this.Component.Key + "]", RenderTextFlags.HorizontalRight | RenderTextFlags.VerticalCenter);


        }

        #endregion
    }
}