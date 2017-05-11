namespace Aimtec.SDK.Menu
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    using Aimtec.SDK.Menu.Theme;
    using Aimtec.SDK.Util;

    /// <summary>
    ///     Class Menu.
    /// </summary>
    /// <seealso cref="Aimtec.SDK.Menu.IMenu" />
    /// <seealso cref="System.Collections.IEnumerable" />
    public class Menu : IMenu, IEnumerable
    {
        #region Fields

        /// <summary>
        ///     The toggled
        /// </summary>
        private bool toggled;

        /// <summary>
        ///     The visible
        /// </summary>
        private bool visible;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Menu" /> class.
        /// </summary>
        /// <param name="internalName">Name of the internal.</param>
        /// <param name="displayName">The display name.</param>
        public Menu(string internalName, string displayName)
        {
            this.InternalName = internalName;
            this.DisplayName = displayName;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the children.
        /// </summary>
        /// <value>The children.</value>
        public Dictionary<string, IMenuComponent> Children { get; } = new Dictionary<string, IMenuComponent>();

        /// <summary>
        ///     Gets or sets the display name.
        /// </summary>
        /// <value>The display name.</value>
        public string DisplayName { get; set; }

        /// <summary>
        ///     Gets or sets the name of the internal.
        /// </summary>
        /// <value>The name of the internal.</value>
        public string InternalName { get; set; }

        /// <summary>
        ///     Gets or sets the position.
        /// </summary>
        /// <value>The position.</value>
        public Vector2 Position { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="IMenuComponent" /> is toggled.
        /// </summary>
        /// <value><c>true</c> if toggled; otherwise, <c>false</c>.</value>
        public bool Toggled
        {
            get
            {
                return this.toggled;
            }
            set
            {
                this.toggled = value;

                foreach (var child in this.Children.Values)
                {
                    child.Visible = value;

                    if (!this.toggled)
                    {
                        child.Toggled = false;
                    }
                }
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="IMenuComponent" /> is visible.
        /// </summary>
        /// <value><c>true</c> if visible; otherwise, <c>false</c>.</value>
        public bool Visible
        {
            get
            {
                return this.visible;
            }
            set
            {
                this.visible = value;

                if (this.toggled)
                {
                    foreach (var child in this.Children.Values)
                    {
                        child.Visible = value;
                    }
                }
            }
        }

        #endregion

        #region Public Indexers

        /// <summary>
        ///     Gets the <see cref="MenuComponent" /> with the specified internal name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>MenuComponent.</returns>
        public MenuComponent this[string name] => this.GetItem(name);

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Adds the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="menu">The menu.</param>
        /// <returns>IMenu.</returns>
        public IMenu Add(IMenuComponent menu)
        {
            this.Children.Add(menu.InternalName, menu);
            return this;
        }

        /// <summary>
        ///     Adds the specified menu.
        /// </summary>
        /// <param name="menu">The menu.</param>
        /// <returns>IMenu.</returns>
        public IMenu Add(IMenu menu)
        {
            return this.Add((IMenuComponent)menu);
        }

        /// <summary>
        ///     Attaches this instance.
        /// </summary>
        /// <returns>IMenu.</returns>
        public IMenu Attach()
        {
            MenuManager.Instance.Add(this);

            return this;
        }

        /// <summary>
        ///     Gets the bounds.
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <returns>Rectangle.</returns>
        public Rectangle GetBounds(Vector2 pos)
        {
            return new Rectangle(
                (int) pos.X,
                (int) pos.Y,
                MenuManager.Instance.Theme.MenuWidth,
                MenuManager.Instance.Theme.MenuHeight);
        }

        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        public IEnumerator GetEnumerator()
        {
            return this.Children.GetEnumerator();
        }

        /// <summary>
        ///     Gets the item.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>MenuComponent.</returns>
        public MenuComponent GetItem(string name)
        {
            return (MenuComponent) this.Children[name];
        }

        /// <summary>
        ///     Gets the render manager.
        /// </summary>
        /// <returns>IRenderManager.</returns>
        public IRenderManager GetRenderManager()
        {
            return MenuManager.Instance.Theme.BuildMenuRenderer(this);
        }

        /// <summary>
        ///     Renders the specified position.
        /// </summary>
        /// <param name="position">The position.</param>
        public void Render(Vector2 position)
        {
            if (this.Visible)
            {
                this.GetRenderManager().Render(position);
            }

            if (!this.Toggled)
            {
                return;
            }

            for (var i = 0; i < this.Children.Values.Count; i++)
            {
                var child = this.Children.Values.ToList()[i];
                child.Position = position
                    + new Vector2(MenuManager.Instance.Theme.MenuWidth, i * MenuManager.Instance.Theme.MenuHeight);
                child.Render(child.Position);
            }
        }

        /// <summary>
        ///     An application-defined function that processes messages sent to a window.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="wparam">Additional message information.</param>
        /// <param name="lparam">Additional message information.</param>
        public void WndProc(uint message, uint wparam, int lparam)
        {
            if (message == (ulong) WindowsMessages.WM_LBUTTONUP && this.Visible)
            {
                var x = lparam & 0xffff;
                var y = lparam >> 16;

                if (this.GetBounds(this.Position).Contains(x, y))
                {
                    this.Toggled = !this.Toggled;

                    // If we're toggled then children are visible
                    foreach (var child in this.Children.Values)
                    {
                        child.Visible = this.Toggled;
                    }
                }
            }

            // Pass message to children
            foreach (var child in this.Children.Values)
            {
                child.WndProc(message, wparam, lparam);
            }
        }

        #endregion
    }
}