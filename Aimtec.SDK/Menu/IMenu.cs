namespace Aimtec.SDK.Menu
{
    using System.Collections.Generic;

    /// <summary>
    ///     Interface IMenu
    /// </summary>
    /// <seealso cref="Aimtec.SDK.Menu.IMenuComponent" />
    public interface IMenu : IMenuComponent
    {
        #region Public Properties

        /// <summary>
        ///     Gets the children.
        /// </summary>
        /// <value>The children.</value>
        Dictionary<string, IMenuComponent> Children { get; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Adds the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="menu">The menu.</param>
        /// <returns>IMenu.</returns>
        IMenu Add(string id, IMenuComponent menu);

        /// <summary>
        ///     Adds the specified menu.
        /// </summary>
        /// <param name="menu">The menu.</param>
        /// <returns>IMenu.</returns>
        IMenu Add(IMenu menu);

        /// <summary>
        ///     Attaches this instance.
        /// </summary>
        /// <returns>IMenu.</returns>
        IMenu Attach();

        #endregion
    }
}