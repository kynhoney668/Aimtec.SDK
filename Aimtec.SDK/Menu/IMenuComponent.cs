namespace Aimtec.SDK.Menu
{
    using System.Drawing;

    using Aimtec.SDK.Menu.Theme;

    /// <summary>
    ///     Interface IMenuComponent
    /// </summary>
    public interface IMenuComponent
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets the display name.
        /// </summary>
        /// <value>The display name.</value>
        string DisplayName { get; set; }

        /// <summary>
        ///     Gets or sets the name of the internal.
        /// </summary>
        /// <value>The name of the internal.</value>
        string InternalName { get; set; }

        /// <summary>
        ///     Gets or sets the position.
        /// </summary>
        /// <value>The position.</value>
        Vector2 Position { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="IMenuComponent" /> is toggled.
        /// </summary>
        /// <value><c>true</c> if toggled; otherwise, <c>false</c>.</value>
        bool Toggled { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="IMenuComponent" /> is visible.
        /// </summary>
        /// <value><c>true</c> if visible; otherwise, <c>false</c>.</value>
        bool Visible { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Gets the bounds.
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <returns>Rectangle.</returns>
        Rectangle GetBounds(Vector2 pos);

        /// <summary>
        ///     Gets the render manager.
        /// </summary>
        /// <returns>IRenderManager.</returns>
        IRenderManager GetRenderManager();

        /// <summary>
        ///     Renders at the specified position.
        /// </summary>
        /// <param name="pos">The position.</param>
        void Render(Vector2 pos);

        /// <summary>
        ///     An application-defined function that processes messages sent to a window.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="wparam">Additional message information.</param>
        /// <param name="lparam">Additional message information.</param>
        void WndProc(uint message, uint wparam, int lparam);

        #endregion
    }
}