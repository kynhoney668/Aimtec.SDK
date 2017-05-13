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

        /// <summary>
        ///     Renders at the specified position.
        /// </summary>
        /// <param name="pos">The position.</param>
        public void Render(Vector2 pos)
        {
            this.Theme.DrawMenuItemBorder(pos);

            var position = pos + DefaultMenuTheme.LineWidth;

            this.Theme.DrawMenuItemBox(position);

            // Todo measure text
            RenderManager.RenderText(
                pos
                + new Vector2(
                    DefaultMenuTheme.TextSpacing,
                    (this.Theme.MenuHeight - DefaultMenuTheme.LineWidth) / 4f + 9 / 2f - 1),
                // not bro science
                Color.FromArgb(207, 195, 149),
                this.Component.DisplayName);

            // Render arrow outline 1 
            RenderManager.RenderLine(
                pos.X + this.Theme.ComponentWidth - DefaultMenuTheme.IndicatorWidth  * 3.5F - DefaultMenuTheme.LineWidth,
                pos.Y,
                pos.X + this.Theme.ComponentWidth - DefaultMenuTheme.IndicatorWidth  * 3.5F - DefaultMenuTheme.LineWidth,
                pos.Y + this.Theme.MenuHeight,
                Color.FromArgb(82, 83, 57));


            // Render arrow outline 2
            RenderManager.RenderLine(
                pos.X + this.Theme.ComponentWidth - DefaultMenuTheme.IndicatorWidth  - DefaultMenuTheme.LineWidth,
                pos.Y,
                pos.X + this.Theme.ComponentWidth - DefaultMenuTheme.IndicatorWidth  - DefaultMenuTheme.LineWidth,
                pos.Y + this.Theme.MenuHeight,
                Color.FromArgb(82, 83, 57));


            var rightBoxPosition = position + new Vector2(this.Theme.ComponentWidth - DefaultMenuTheme.IndicatorWidth  - DefaultMenuTheme.LineWidth, 0);
            var leftBoxPosition = position + new Vector2(this.Theme.ComponentWidth - DefaultMenuTheme.IndicatorWidth  * 3.5f - DefaultMenuTheme.LineWidth, 0);

            //var center = Vector2.Distance(rightBoxPosition, leftBoxPosition) / 2;

            var valuePos = leftBoxPosition + new Vector2(DefaultMenuTheme.IndicatorWidth  + 7.5f, 0);

            RenderManager.RenderText(
                valuePos
                + new Vector2(0,
                (this.Theme.MenuHeight - DefaultMenuTheme.LineWidth) / 4f + 9 / 2f - 1),
                Color.FromArgb(207, 195, 149),
                this.Component.Items[this.Component.Value]);


            // Draw arrow boxex

            RenderManager.RenderRectangle(
                leftBoxPosition,
                DefaultMenuTheme.IndicatorWidth ,
                this.Theme.MenuHeight - DefaultMenuTheme.LineWidth,
                Color.FromArgb(16, 26, 29));

            // todo center text in box
            RenderManager.RenderText(
                leftBoxPosition + new Vector2(5, 10),
                Color.AliceBlue,
                "<");

            RenderManager.RenderRectangle(
                rightBoxPosition,
                DefaultMenuTheme.IndicatorWidth ,
                this.Theme.MenuHeight - DefaultMenuTheme.LineWidth,
                Color.FromArgb(16, 26, 29));


            RenderManager.RenderText(
                rightBoxPosition + new Vector2(5, 10),
                Color.AliceBlue,
                ">");

            #endregion
        }
    }
}
