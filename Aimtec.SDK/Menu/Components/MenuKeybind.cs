namespace Aimtec.SDK.Menu.Components
{
    using System;
    using System.Drawing;
    using Aimtec.SDK.Menu.Theme;
    using Aimtec.SDK.Util;

    /// <summary>
    ///     Class MenuKeybind. This class cannot be inherited.
    /// </summary>
    /// <seealso cref="Aimtec.SDK.Menu.MenuComponent" />
    /// <seealso cref="bool" />
    public sealed class MenuKeybind : MenuComponent, IReturns<bool>
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="MenuKeybind" /> class.
        /// </summary>
        /// <param name="internalName">The internal name.</param>
        /// <param name="displayName">The display name.</param>
        /// <param name="key">The key.</param>
        /// <param name="keybindType">Type of the keybind.</param>
        public MenuKeybind(string internalName, string displayName, Keys key, KeybindType keybindType)
        {
            this.InternalName = internalName;
            this.DisplayName = displayName;
            this.Key = key;
            this.KeybindType = keybindType;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the display name.
        /// </summary>
        /// <value>The display name.</value>
        public override string DisplayName { get; set; }

        /// <summary>
        ///     Gets or sets the key.
        /// </summary>
        /// <value>The key.</value>
        public Keys Key { get; set; }

        /// <summary>
        ///     Gets or sets the type of the keybind.
        /// </summary>
        /// <value>The type of the keybind.</value>
        public KeybindType KeybindType { get; set; }

        /// <summary>
        ///     Gets or sets the position.
        /// </summary>
        /// <value>The position.</value>
        public override Vector2 Position { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="IMenuComponent" /> is toggled.
        /// </summary>
        /// <value><c>true</c> if toggled; otherwise, <c>false</c>.</value>
        public override bool Toggled { get; set; }

        /// <summary>
        ///     Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public bool Value { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="IMenuComponent" /> is visible.
        /// </summary>
        /// <value><c>true</c> if visible; otherwise, <c>false</c>.</value>
        public override bool Visible { get; set; }

        public bool KeyIsBeingSet { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Gets the render manager.
        /// </summary>
        /// <returns>Aimtec.SDK.Menu.Theme.IRenderManager.</returns>
        public override IRenderManager GetRenderManager()
        {
            return MenuManager.Instance.Theme.BuildMenuKeybindRenderer(this);
        }

        /// <summary>
        ///     An application-defined function that processes messages sent to a window.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="wparam">Additional message information.</param>
        /// <param name="lparam">Additional message information.</param>
        public override void WndProc(uint message, uint wparam, int lparam)
        {
            if (this.Visible)
            {
                var x = lparam & 0xffff;
                var y = lparam >> 16;

                if (message == (ulong)WindowsMessages.WM_LBUTTONDOWN)
                {
                    if (!this.KeyIsBeingSet && this.GetBounds(this.Position).Contains(x, y))
                    {
                        if (!MenuManager.Instance.Theme.GetControlObjectBounds(this.Position, MenuTheme.MenuItemType.MenuBool).Contains(x, y))
                        {
                            this.KeyIsBeingSet = true;
                        }

                        else
                        {
                            this.Value = !this.Value;
                        }
                    }
                }

                if (this.KeyIsBeingSet && message == (ulong)WindowsMessages.WM_KEYUP)
                {
                    this.Key = (Keys)wparam;
                    this.KeyIsBeingSet = false;
                }
            }

            if (wparam != (ulong)this.Key || KeyIsBeingSet)
            {
                return;
            }

            if (this.KeybindType == KeybindType.Press)
            {
                if (message == (ulong)WindowsMessages.WM_KEYDOWN)
                {
                    this.Value = true;
                }
                else if (message == (ulong)WindowsMessages.WM_KEYUP)
                {
                    this.Value = false;
                }
            }

            else if (message == (ulong)WindowsMessages.WM_KEYUP)
            {
                this.Value = !this.Value;
            }
        }

        #endregion
    }

    /// <summary>
    ///     Enum KeybindType
    /// </summary>
    public enum KeybindType
    {
        /// <summary>
        ///     Press key bind.
        /// </summary>
        Press,

        /// <summary>
        ///     Toggle key bind.
        /// </summary>
        Toggle
    }
}