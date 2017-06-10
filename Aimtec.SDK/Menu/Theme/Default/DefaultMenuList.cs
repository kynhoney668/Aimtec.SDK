namespace Aimtec.SDK.Menu.Theme.Default
{
    using System.Drawing;
    using Aimtec.SDK.Extensions;

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

        /// <summary>
        ///     Renders at the specified position.
        /// </summary>
        /// <param name="pos">The position.</param>
        public void Render(Vector2 pos)
        {
            var width = this.Component.Parent.Width;

            this.Theme.DrawMenuItemBorder(pos, width);

            var position = pos + DefaultMenuTheme.LineWidth;

            this.Theme.DrawMenuItemBox(position, width);

            var displayNamePosition = position + new Vector2(DefaultMenuTheme.TextSpacing, (this.Theme.MenuHeight) / 2);

            RenderManager.RenderText(
                displayNamePosition,
                Color.FromArgb(207, 195, 149),
                this.Component.DisplayName, RenderTextFlags.VerticalCenter);

            // Render arrow outline 1 - left arrow 
            RenderManager.RenderLine(
                pos.X + width - DefaultMenuTheme.IndicatorWidth  * 2.1F - DefaultMenuTheme.LineWidth,
                pos.Y,
                pos.X + width - DefaultMenuTheme.IndicatorWidth  * 2.1F - DefaultMenuTheme.LineWidth,
                pos.Y + this.Theme.MenuHeight,
                Color.FromArgb(82, 83, 57));

  
            // Render arrow outline 2 - right arrow
            RenderManager.RenderLine(
                pos.X + width - DefaultMenuTheme.IndicatorWidth - DefaultMenuTheme.LineWidth,
                pos.Y,
                pos.X + width - DefaultMenuTheme.IndicatorWidth - DefaultMenuTheme.LineWidth,
                pos.Y + this.Theme.MenuHeight,
                Color.FromArgb(82, 83, 57));


            var leftBoxPosition = position + new Vector2(width - DefaultMenuTheme.IndicatorWidth * 2.1f - DefaultMenuTheme.LineWidth, 0);
            var rightBoxPosition = position + new Vector2(width - DefaultMenuTheme.IndicatorWidth  - DefaultMenuTheme.LineWidth, 0);

            RenderManager.RenderText(
                leftBoxPosition
                + new Vector2(-DefaultMenuTheme.TextSpacing, (this.Theme.MenuHeight) / 2),
                Color.FromArgb(207, 195, 149),
                this.Component.Items[this.Component.Value], RenderTextFlags.VerticalCenter | RenderTextFlags.HorizontalRight);


            // Draw arrow boxes

            RenderManager.RenderRectangle(
                leftBoxPosition,
                DefaultMenuTheme.IndicatorWidth,
                this.Theme.MenuHeight - DefaultMenuTheme.LineWidth,
                Color.FromArgb(16, 26, 29));

            RenderManager.RenderText(
                leftBoxPosition + new Vector2(DefaultMenuTheme.IndicatorWidth / 2, this.Theme.MenuHeight / 2),
                Color.FromArgb(207, 195, 149),
                "<", RenderTextFlags.HorizontalCenter | RenderTextFlags.VerticalCenter);

            RenderManager.RenderRectangle(
                rightBoxPosition,
                DefaultMenuTheme.IndicatorWidth,
                this.Theme.MenuHeight - DefaultMenuTheme.LineWidth,
                Color.FromArgb(16, 26, 29));

            RenderManager.RenderText(
                rightBoxPosition + new Vector2(DefaultMenuTheme.IndicatorWidth / 2, this.Theme.MenuHeight / 2),
                Color.FromArgb(207, 195, 149),
                ">", RenderTextFlags.HorizontalCenter | RenderTextFlags.VerticalCenter);

            #endregion
        }
    }
}
