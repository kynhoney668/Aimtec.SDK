namespace Aimtec.SDK.Menu.Components
{
    using System;

    using Aimtec.SDK.Menu.Theme;
    using Util;

    /// <summary>
    ///     Class MenuList.
    /// </summary>
    /// <seealso cref="Aimtec.SDK.Menu.MenuComponent" />
    public sealed class MenuList : MenuComponent, IReturns<int>
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="MenuList" /> class.
        /// </summary>
        /// <param name="internalName">The Internal Name.</param>
        /// <param name="displayName">The Displayed Name</param>
        /// <param name="items">The items.</param>
        /// <param name="selectedValue">The selected value.</param>
        /// <exception cref="System.ArgumentException">selectedValue</exception>
        public MenuList(string internalName, string displayName, string[] items, int selectedValue)
        {
            if (items.Length < selectedValue - 1)
            {
                throw new ArgumentException($"{nameof(selectedValue)} is outside the bounds of {nameof(items)}");
            }

            this.InternalName = internalName;
            this.DisplayName = displayName;
            this.Value = selectedValue;
            this.Items = items;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the display name.
        /// </summary>
        /// <value>The display name.</value>
        public override string DisplayName { get; set; }

        /// <summary>
        ///     Gets or sets the items.
        /// </summary>
        /// <value>The items.</value>
        public string[] Items { get; set; }

        public override Menu Parent { get; set; }

        /// <summary>
        ///     Gets or sets the position.
        /// </summary>
        /// <value>The position.</value>
        public override Vector2 Position { get; set; }

        public override bool Root { get; set; }

        /// <summary>
        ///     Gets the selected item.
        /// </summary>
        /// <value>The selected item.</value>
        public string SelectedItem => this.Items[this.Value];

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="IMenuComponent" /> is toggled.
        /// </summary>
        /// <value><c>true</c> if toggled; otherwise, <c>false</c>.</value>
        public override bool Toggled { get; set; }

        /// <summary>
        ///     Gets or sets the value, which is the index of the string list.
        /// </summary>
        /// <value>The value.</value>
        public int Value { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="IMenuComponent" /> is visible.
        /// </summary>
        /// <value><c>true</c> if visible; otherwise, <c>false</c>.</value>
        public override bool Visible { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Gets the render manager.
        /// </summary>
        /// <returns>Aimtec.SDK.Menu.Theme.IRenderManager.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public override IRenderManager GetRenderManager()
        {
            return MenuManager.Instance.Theme.BuildMenuList(this);
        }

        /// <summary>
        ///     An application-defined function that processes messages sent to a window.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="wparam">Additional message information.</param>
        /// <param name="lparam">Additional message information.</param>
        public override void WndProc(uint message, uint wparam, int lparam)
        {
            if (message == (ulong)WindowsMessages.WM_LBUTTONUP && this.Visible)
            {
                var x = lparam & 0xffff;
                var y = lparam >> 16;

                var controls = MenuManager.Instance.Theme.GetMenuListControlBounds(this.Position);
                var leftControl = controls[0];
                var rightControl = controls[1];

                if (leftControl.Contains(x, y))
                {
                    if (this.Value == 0)
                    {
                        this.Value = Items.Length - 1;
                    }

                    else
                    {
                        this.Value -= 1;
                    }
                }

                else if (rightControl.Contains(x, y))
                {
                    if (this.Value == Items.Length - 1)
                    {
                        this.Value = 0;
                    }

                    else
                    {
                        this.Value += 1;
                    }
                }
            }
        }

        #endregion
    }
}