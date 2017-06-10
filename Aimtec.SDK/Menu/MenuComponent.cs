namespace Aimtec.SDK.Menu
{
    using Aimtec.SDK.Menu.Theme;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;

    using Newtonsoft.Json;

    /// <summary>
    ///     Class MenuComponent.
    /// </summary>
    /// <seealso cref="Aimtec.SDK.Menu.IMenuComponent" />
    public abstract class MenuComponent : IMenuComponent
    {
        #region Public Properties

        /// <summary>
        ///     Gets the children.
        /// </summary>
        /// <value>The children.</value>
        public virtual Dictionary<string, MenuComponent> Children { get; }

        public virtual MenuComponent this[string name] => null;

        internal virtual string ConfigName => this.CleanFileName(
            $"{this.DisplayName}-{this.InternalName}-{this.GetType().Name}");

        internal virtual bool SavableMenuItem { get; set; } = true;

        internal string ConfigBaseFolder
        {
            get
            {
                if (this.Shared || this.Parent != null && this.Parent.Shared)
                {
                    return MenuManager.Instance.SharedSettingsPath;
                }

                return Path.Combine(MenuManager.Instance.MenuSettingsPath, this.CallingAssemblyName);
            }
        }


        public string ConfigPath
        {
            get
            {
                if (this.Root || this.Shared)
                {
                    return Path.Combine(this.ConfigBaseFolder, this.ConfigName);
                }

                if (this.IsMenu)
                {
                    return Path.Combine(this.Parent.ConfigPath, this.ConfigName);
                }

                return Path.Combine(this.Parent.ConfigPath, this.ConfigName + ".json");
            }
        }


        internal string CallingAssemblyName { get; set; }

        internal abstract string Serialized { get; }

        public delegate void ValueChangedHandler(MenuComponent sender, ValueChangedArgs args);

        public virtual event ValueChangedHandler ValueChanged;

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

        /// <summary>
        ///     Gets or sets the display name.
        /// </summary>
        /// <value>The display name.</value>
        [JsonProperty(Order = 1)]
        public string DisplayName { get; set; }

        /// <summary>
        ///     Gets or sets the name of the internal.
        /// </summary>
        /// <value>The name of the internal.</value>
        [JsonProperty(Order = 2)]
        public string InternalName { get; set; }

        /// <summary>
        ///     Gets a value indicating whether this <see cref="MenuComponent" /> is enabled.
        /// </summary>
        /// <remarks>
        ///    This property will only succeed for MenuBool, MenuKeybind and MenuSliderBool.
        /// </remarks>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        public bool Enabled => ((IReturns<bool>)this).Value;

        /// <summary>
        ///     Gets a numeric value associated with MenuComponent <see cref="MenuComponent" />.
        /// </summary>
        /// <remarks>
        ///     This property will only succeed for MenuSlider, MenuSliderBool and MenuList.
        /// </remarks>
        public int Value => ((IReturns<int>)this).Value;


        /// <summary>
        ///     Gets or sets the position.
        /// </summary>
        /// <value>The position.</value>
        public Vector2 Position { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="IMenuComponent" /> is toggled.
        /// </summary>
        /// <value><c>true</c> if toggled; otherwise, <c>false</c>.</value>
        public virtual bool Toggled { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="IMenuComponent" /> is visible.
        /// </summary>
        /// <value><c>true</c> if visible; otherwise, <c>false</c>.</value>
        public virtual bool Visible { get; set; }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>The parent.</value>
        public Menu Parent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="MenuComponent"/> is the root menu.
        /// </summary>
        /// <value><c>true</c> if this menu is the root menu; otherwise, <c>false</c>.</value>
        public bool Root { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is a menu.
        /// </summary>
        /// <value><c>true</c> if this instance is a menu; otherwise, <c>false</c>.</value>
        public virtual bool IsMenu => false;

        public bool Shared { get; set; }

        #endregion

        #region Public Methods and Operators

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

            throw new Exception($"[Menu] Item: {name} was not found in the menu: {this.InternalName}");
        }

        /// <summary>
        ///     Converts the <see cref="MenuComponent" /> to the specified <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>T.</returns>
        public T As<T>() where T : MenuComponent
        {
            return (T)this;
        }

        /// <summary>
        ///     Gets the bounds.
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <returns>System.Drawing.Rectangle.</returns>
        public virtual Rectangle GetBounds(Vector2 pos)
        {
            return new Rectangle(
                (int)pos.X,
                (int)pos.Y,
                this.Parent.Width,
                MenuManager.Instance.Theme.MenuHeight);
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
                this.GetRenderManager().Render(pos);
            }
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

        internal abstract void LoadValue();

        private string CleanFileName(string fileName)
        {
            var clean = Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
            return clean;
        }

        #endregion
    }
}
