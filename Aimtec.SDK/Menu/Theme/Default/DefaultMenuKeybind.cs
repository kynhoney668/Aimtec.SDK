namespace Aimtec.SDK.Menu.Theme.Default
{
    using System.Drawing;

    using Aimtec.SDK.Menu.Components;

    using Rectangle = Aimtec.Rectangle;

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
            var width = this.Component.Parent.Width;
            var height = MenuManager.MaxHeightItem + this.Theme.BonusMenuHeight;

            this.Theme.DrawMenuItemBorder(pos, width);

            var position = pos + this.Theme.LineWidth;

            this.Theme.DrawMenuItemBox(position, width);

            var leftVCenter = RenderTextFlags.VerticalCenter | RenderTextFlags.HorizontalLeft | RenderTextFlags.NoClip;

            var displayNamePosition = new Aimtec.Rectangle((int)position.X + this.Theme.TextSpacing, (int)position.Y, (int)(position.X + width), (int)(position.Y + height));

            Aimtec.Render.Text(this.Component.DisplayName + (!string.IsNullOrEmpty(this.Component.ToolTip) ? " [?]" : ""),
                displayNamePosition,
                leftVCenter, this.Theme.TextColor);

            // Render indicator box outline
            Aimtec.Render.Line(
                pos.X + width - this.Theme.IndicatorWidth - this.Theme.LineWidth,
                pos.Y,
                pos.X + width - this.Theme.IndicatorWidth - this.Theme.LineWidth,
                pos.Y + height,
                Color.FromArgb(82, 83, 57));

            // Draw indicator box
            var indBoxPosition = position + new Vector2(width - this.Theme.IndicatorWidth - this.Theme.LineWidth, 0);

            var boolColor = this.Component.Value ? Color.FromArgb(39, 96, 17) : Color.FromArgb(85, 25, 15);

            Aimtec.Render.Rectangle(
                indBoxPosition,
                this.Theme.IndicatorWidth,
                height - this.Theme.LineWidth,
                boolColor);

            var centerArrowBox = new Aimtec.Rectangle((int)indBoxPosition.X, (int)indBoxPosition.Y, (int)(indBoxPosition.X + this.Theme.IndicatorWidth), (int)(indBoxPosition.Y + height));

            Aimtec.Render.Text(this.Component.Value ? "ON" : "OFF", centerArrowBox, RenderTextFlags.HorizontalCenter | RenderTextFlags.VerticalCenter | RenderTextFlags.NoClip | RenderTextFlags.SingleLine, Color.White);

            //Draw Key indicator

            var text = this.Component.KeyIsBeingSet
                ? "PRESS KEY"
                : this.Component.Inactive
                    ? "None"
                    : $"[{this.Component.Key}]";

            Aimtec.Render.Text(text, new Aimtec.Rectangle((int)pos.X, (int)pos.Y, (int)(pos.X + width - this.Theme.IndicatorWidth - this.Theme.LineWidth - this.Theme.TextSpacing), (int)(pos.Y + height)), RenderTextFlags.VerticalCenter | RenderTextFlags.HorizontalRight | RenderTextFlags.NoClip | RenderTextFlags.SingleLine, this.Theme.TextColor);

        }

        #endregion
    }
}