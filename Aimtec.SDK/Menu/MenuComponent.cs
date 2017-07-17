namespace Aimtec.SDK.Menu
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;

    using Aimtec.SDK.Menu.Theme;
    using Aimtec.SDK.Menu.Theme.Default;
    using Aimtec.SDK.Util;

    using Newtonsoft.Json;

    using NLog;

    /// <summary>
    ///     Class MenuComponent.
    /// </summary>
    /// <seealso cref="Aimtec.SDK.Menu.IMenuComponent" />
    public abstract class MenuComponent : IMenuComponent
    {
        #region Fields

        private string _AssemblyConfigDirectoryName;

        #endregion

        #region Delegates

        public delegate void ValueChangedHandler(MenuComponent sender, ValueChangedArgs args);

        #endregion

        #region Public Events

        public virtual event ValueChangedHandler ValueChanged;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the children.
        /// </summary>
        /// <value>The children.</value>
        public virtual Dictionary<string, MenuComponent> Children { get; }

        /// <summary>
        ///     Gets or sets the display name.
        /// </summary>
        /// <value>The display name.</value>
        [JsonProperty(Order = 1)]
        public string DisplayName { get; set; }

        /// <summary>
        ///     Gets a value indicating whether this <see cref="MenuComponent" /> is enabled.
        /// </summary>
        /// <remarks>
        ///     This property will only succeed for MenuBool, MenuKeybind and MenuSliderBool.
        /// </remarks>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        public bool Enabled => ((IReturns<bool>) this).Value;

        /// <summary>
        ///     Gets or sets the name of the internal.
        /// </summary>
        /// <value>The name of the internal.</value>
        [JsonProperty(Order = 2)]
        public string InternalName { get; set; }

        /// <summary>
        ///     Gets a value indicating whether this instance is a menu.
        /// </summary>
        /// <value><c>true</c> if this instance is a menu; otherwise, <c>false</c>.</value>
        public virtual bool IsMenu => false;

        /// <summary>
        ///     Gets or sets the parent.
        /// </summary>
        /// <value>The parent.</value>
        public Menu Parent { get; set; }

        /// <summary>
        ///     Gets or sets the position.
        /// </summary>
        /// <value>The position.</value>
        public Vector2 Position { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="MenuComponent" /> is the root menu.
        /// </summary>
        /// <value><c>true</c> if this menu is the root menu; otherwise, <c>false</c>.</value>
        public bool Root { get; set; }

        public bool Shared { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="IMenuComponent" /> is toggled.
        /// </summary>
        /// <value><c>true</c> if toggled; otherwise, <c>false</c>.</value>
        public virtual bool Toggled { get; set; }

        /// <summary>
        ///     Gets a numeric value associated with MenuComponent <see cref="MenuComponent" />.
        /// </summary>
        /// <remarks>
        ///     This property will only succeed for MenuSlider, MenuSliderBool and MenuList.
        /// </remarks>
        public int Value => ((IReturns<int>) this).Value;

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="IMenuComponent" /> is visible.
        /// </summary>
        /// <value><c>true</c> if visible; otherwise, <c>false</c>.</value>
        public virtual bool Visible { get; set; }

        #endregion

        #region Properties

        internal string AssemblyConfigDirectoryName
        {
            get
            {
                return this.Root || this.Parent == null
                    ? this._AssemblyConfigDirectoryName
                    : this.Parent.AssemblyConfigDirectoryName;
            }
            set
            {
                this._AssemblyConfigDirectoryName = value;
            }
        }

        internal string ConfigBaseFolder
        {
            get
            {
                if (this.Shared || this.Parent != null && this.Parent.Shared)
                {
                    return MenuManager.Instance.SharedSettingsPath;
                }

                return Path.Combine(MenuManager.Instance.MenuSettingsPath, this.AssemblyConfigDirectoryName);
            }
        }

        internal virtual string ConfigName
        {
            get
            {
                if (this.IsMenu)
                {
                    return this.CleanFileName(this.InternalName);
                }

                return this.CleanFileName($"{this.InternalName}.{this.GetType().Name}.json");
            }
        }

        internal string ConfigPath
        {
            get
            {
                if (this.Root || this.Shared)
                {
                    return Path.Combine(this.ConfigBaseFolder, this.ConfigName);
                }

                return Path.Combine(this.Parent.ConfigPath, this.ConfigName);
            }
        }

        /// <summary>
        ///     Gets the Root Menu that this Menu belongs to
        /// </summary>
        internal Menu RootMenu
        {
            get
            {
                if (this.Root)
                {
                    return (Menu) this;
                }

                if (this.Parent != null)
                {
                    return this.Parent.RootMenu;
                }

                return null;
            }
        }

        internal virtual bool SavableMenuItem { get; set; } = true;

        internal abstract string Serialized { get; }

        protected static Logger Logger => LogManager.GetCurrentClassLogger();

        private string ToolTip { get; set; }

        #endregion

        #region Public Indexers

        public virtual MenuComponent this[string name] => null;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Converts the <see cref="MenuComponent" /> to the specified <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>T.</returns>
        public T As<T>()
            where T : MenuComponent
        {
            return (T) this;
        }

        /// <summary>
        ///     The WndProc that all Menu Components share
        /// </summary>
        public void BaseWndProc(uint message, uint wparam, int lparam)
        {
            if (this.Visible && message == (ulong) WindowsMessages.WM_MOUSEMOVE)
            {
                var x = lparam & 0xffff;
                var y = lparam >> 16;

                if (this.GetBounds(this.Position).Contains(x, y))
                {
                    MenuManager.LastMouseMoveTime = Game.TickCount;
                    MenuManager.LastMousePosition = new Point(x, y);
                }
            }

            this.WndProc(message, wparam, lparam);
        }

        /// <summary>
        ///     Removes this component from its parent menu
        /// </summary>
        public void Dispose()
        {
            if (this.Parent != null)
            {
                this.Parent.Children.Remove(this.InternalName);
            }
        }

        /// <summary>
        ///     Gets the bounds.
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <returns>System.Drawing.Rectangle.</returns>
        public virtual Rectangle GetBounds(Vector2 pos)
        {
            return new Rectangle((int) pos.X, (int) pos.Y, this.Parent.Width, MenuManager.Instance.Theme.MenuHeight);
        }

        /// <summary>
        ///     Gets the item.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>MenuComponent.</returns>
        public MenuComponent GetItem(string name)
        {
            if (this.Children.Keys.Contains(name))
            {
                return this.Children[name];
            }

            Logger.Warn("[Menu] Item: {0} was not found in the menu: {1}", name, this.InternalName);

            return null;
        }

        /// <summary>
        ///     Gets the render manager.
        /// </summary>
        /// <returns>Aimtec.SDK.Menu.Theme.IRenderManager.</returns>
        public abstract IRenderManager GetRenderManager();

        /// <summary>
        ///     Renders at the specified position.
        /// </summary>
        /// <param name="pos">The position.</param>
        public virtual void Render(Vector2 pos)
        {
            if (this.Visible)
            {
                if (!string.IsNullOrEmpty(this.ToolTip))
                {
                    if (Game.TickCount - MenuManager.LastMouseMoveTime > 500)
                    {
                        if (this.GetBounds(this.Position).Contains(MenuManager.LastMousePosition))
                        {
                            this.RenderToolTip();
                            return;
                        }
                    }
                }

                this.GetRenderManager().Render(pos);
            }
        }

        /// <summary>
        ///     Renders the tooltip
        /// </summary>
        public void RenderToolTip()
        {
            var text = $"[i] {this.ToolTip}";
            var width = Math.Max(this.Parent.Width, (int) MenuManager.Instance.TextWidth(text));

            DefaultMenuTheme.DrawRectangleOutline(
                this.Position.X,
                this.Position.Y,
                width,
                MenuManager.Instance.Theme.MenuHeight,
                MenuManager.Instance.Theme.LineWidth,
                MenuManager.Instance.Theme.LineColor);

            var position = this.Position + MenuManager.Instance.Theme.LineWidth;

            Aimtec.Render.Rectangle(
                position,
                width - MenuManager.Instance.Theme.LineWidth,
                MenuManager.Instance.Theme.MenuHeight - MenuManager.Instance.Theme.LineWidth,
                MenuManager.Instance.Theme.MenuBoxBackgroundColor);

            var centerPoint = this.Position + new Vector2(
                width - MenuManager.Instance.Theme.LineWidth * 2 / 2,
                MenuManager.Instance.Theme.MenuHeight - MenuManager.Instance.Theme.LineWidth * 2 / 2);

            var textPosition = position + new Vector2(
                MenuManager.Instance.Theme.TextSpacing,
                MenuManager.Instance.Theme.MenuHeight / 2);
            Aimtec.Render.Text(textPosition, Color.LightBlue, text, RenderTextFlags.VerticalCenter);
        }

        /// <summary>
        ///     Sets the Tool Tip
        /// </summary>
        /// <param name="toolTip">The tooltip</param>
        public MenuComponent SetToolTip(string toolTip)
        {
            this.ToolTip = toolTip;
            return this;
        }

        /// <summary>
        ///     An application-defined function that processes messages sent to a window.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="wparam">Additional message information.</param>
        /// <param name="lparam">Additional message information.</param>
        public virtual void WndProc(uint message, uint wparam, int lparam)
        {
        }

        #endregion

        #region Methods

        internal abstract void LoadValue();

        internal virtual void Save()
        {
            //no point to save things like seperators
            if (!this.SavableMenuItem)
            {
                return;
            }

            if (this.Shared)
            {
                if (!Directory.Exists(MenuManager.Instance.SharedSettingsPath))
                {
                    Directory.CreateDirectory(MenuManager.Instance.SharedSettingsPath);
                }
            }

            else
            {
                if (!Directory.Exists(this.Parent.ConfigPath))
                {
                    Directory.CreateDirectory(this.Parent.ConfigPath);
                }
            }

            File.WriteAllText(this.ConfigPath, this.Serialized);
        }

        protected virtual void FireOnValueChanged(MenuComponent sender, ValueChangedArgs args)
        {
            if (this.ValueChanged != null)
            {
                //Fire the value changed of this menucomponent instance
                this.ValueChanged(sender, args);
            }

            //Fire the value changed on the parent menu 
            if (this.Parent != null)
            {
                this.Parent.FireOnValueChanged(sender, args);
            }
        }

        private string CleanFileName(string fileName)
        {
            var clean = Path.GetInvalidFileNameChars().Aggregate(
                fileName,
                (current, c) => current.Replace(c.ToString(), string.Empty));
            return clean;
        }

        #endregion
    }
}