namespace Aimtec.SDK.Menu
{
    using System.Collections.Generic;
    using System.Drawing;

    using Aimtec.SDK.Menu.Theme;

    /// <summary>
    ///     Interface IMenuComponent
    /// </summary>
    public interface IMenuComponent
    {
        #region Public Properties

        /// <summary>
        ///     Converts the <see cref="IMenuComponent" /> to the specified <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>T.</returns>
        T As<T>() where T : MenuComponent;


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
        ///     Gets a value indicating whether this <see cref="MenuComponent" /> is enabled.
        /// </summary>
        /// <remarks>
        ///     This property will only succeed if the MenuComponent implements <see cref="IReturns{bool}" />.
        ///     This property will only succeed for MenuBool, MenuKeyBind and MenuSliderBool" />.
        /// </remarks>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        bool Enabled { get; }

        /// <summary>
        ///     Gets a value associated with this <see cref="MenuComponent" />
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        int Value { get; }

        /// <summary>
        ///     Gets or sets whether this <see cref="MenuComponent" /> is a a root menu.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        bool Root { get; set; }

        /// <summary>
        ///     Gets the parent of this <see cref="MenuComponent" /> if it has one.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        Menu Parent { get; set; }

        /// <summary>
        ///     Gets or sets whether this <see cref="MenuComponent" /> is a Menu.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        bool IsMenu { get; }

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