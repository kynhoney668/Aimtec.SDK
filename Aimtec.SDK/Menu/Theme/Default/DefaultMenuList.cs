namespace Aimtec.SDK.Menu.Theme.Default
{
    using System.Drawing;

    using Aimtec.SDK.Menu.Components;

    /// <summary>
    ///     Class DefaultMenuList.
    /// </summary>
    /// <seealso
    ///     cref="Aimtec.SDK.Menu.Theme.IRenderManager{MenuList, DefaultMenuTheme}" />
    internal class DefaultMenuList : IRenderManager<MenuList, DefaultMenuTheme>
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="DefaultMenuList" /> class.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <param name="theme">The theme.</param>
        public DefaultMenuList(MenuList component, DefaultMenuTheme theme)
        {
            this.Component = component;
            this.Theme = theme;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the component.
        /// </summary>
        /// <value>The component.</value>
        public MenuList Component { get; }

        /// <summary>
        ///     Gets the theme.
        /// </summary>
        /// <value>The theme.</value>
        public DefaultMenuTheme Theme { get; }

        #endregion

        #region Public Methods and Operators

        #region Public Methods and Operators

        /// <summary>
        ///     Renders at the specified position.
        /// </summary>
        /// <param name="pos">The position.</param>
        public void Render(Vector2 pos)
        {
            var width = this.Component.Parent.Width;
            var height = MenuManager.MaxHeightItem + this.Theme.BonusMenuHeight;

            this.Theme.DrawMenuItemBorder(pos, width);

            var position = pos + this.Theme.LineWidth;

            this.Theme.DrawMenuItemBox(position, width);

            var leftVCenter = RenderTextFlags.VerticalCenter | RenderTextFlags.HorizontalLeft | RenderTextFlags.NoClip
                              | RenderTextFlags.SingleLine;

            var displayNamePosition = new Aimtec.Rectangle((int)position.X + this.Theme.TextSpacing, (int)position.Y, (int)(position.X + width), (int)(position.Y + height));

            Aimtec.Render.Text(this.Component.DisplayName + (!string.IsNullOrEmpty(this.Component.ToolTip) ? " [?]" : ""),
                displayNamePosition,
                leftVCenter, this.Theme.TextColor);

            // Render arrow outline 1 - left arrow 
            Aimtec.Render.Line(
                pos.X + width - this.Theme.IndicatorWidth * 2.1F - this.Theme.LineWidth,
                pos.Y,
                pos.X + width - this.Theme.IndicatorWidth * 2.1F - this.Theme.LineWidth,
                pos.Y + height,
                Color.FromArgb(82, 83, 57));

            // Render arrow outline 2 - right arrow
            Aimtec.Render.Line(
                pos.X + width - this.Theme.IndicatorWidth - this.Theme.LineWidth,
                pos.Y,
                pos.X + width - this.Theme.IndicatorWidth - this.Theme.LineWidth,
                pos.Y + height,
                Color.FromArgb(82, 83, 57));

            var leftBoxPosition = position + new Vector2(
                width - this.Theme.IndicatorWidth * 2.1f - this.Theme.LineWidth,
                0);

            var rightBoxPosition = position + new Vector2(width - this.Theme.IndicatorWidth - this.Theme.LineWidth, 0);

            // Draw arrow boxes
            Aimtec.Render.Rectangle(
                leftBoxPosition,
                this.Theme.IndicatorWidth,
                height - this.Theme.LineWidth,
                Color.FromArgb(16, 26, 29));

            var rectLeft = new Aimtec.Rectangle((int)leftBoxPosition.X, (int)leftBoxPosition.Y, (int) (leftBoxPosition.X + this.Theme.IndicatorWidth), (int) (leftBoxPosition.Y + height));
            Aimtec.Render.Text("<",
                rectLeft,
                RenderTextFlags.HorizontalCenter | RenderTextFlags.VerticalCenter, this.Theme.TextColor);

            Aimtec.Render.Rectangle(
                rightBoxPosition,
                this.Theme.IndicatorWidth,
                height - this.Theme.LineWidth,
                Color.FromArgb(16, 26, 29));

            var rectRight = new Aimtec.Rectangle((int)rightBoxPosition.X, (int)rightBoxPosition.Y, (int)(rightBoxPosition.X + this.Theme.IndicatorWidth), (int)(rightBoxPosition.Y + height));

            Aimtec.Render.Text(">",
                rectRight,
                RenderTextFlags.HorizontalCenter | RenderTextFlags.VerticalCenter, this.Theme.TextColor);

            var valuePosition = new Aimtec.Rectangle((int)position.X + this.Theme.TextSpacing, (int)position.Y, (int)(position.X + width - this.Theme.IndicatorWidth * 2 - this.Theme.LineWidth * 2 - 15), (int)(position.Y + height));

            Aimtec.Render.Text(this.Component.Items[this.Component.Value],
                valuePosition,
                RenderTextFlags.VerticalCenter | RenderTextFlags.HorizontalRight, this.Theme.TextColor);

            #endregion
        }

        #endregion
    }
}