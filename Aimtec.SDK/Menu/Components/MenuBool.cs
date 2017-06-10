namespace Aimtec.SDK.Menu.Components
{
    using System;
    using System.IO;
    using System.Reflection;

    using Aimtec.SDK.Menu.Theme;
    using Aimtec.SDK.Util;

    using Newtonsoft.Json;

    /// <summary>
    ///     Class MenuBool. This class cannot be inherited.
    /// </summary>
    /// <seealso cref="Aimtec.SDK.Menu.MenuComponent" />
    /// <seealso cref="bool" />
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class MenuBool : MenuComponent, IReturns<bool>
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="MenuBool" /> class.
        /// </summary>
        /// <param name="internalName">The display name.</param>
        /// <param name="displayName">The display name.</param>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        /// <param name="shared">if set to <c>true</c> [shared].</param>
        public MenuBool(string internalName, string displayName, bool enabled = true, bool shared = false)
        {
            this.InternalName = internalName;
            this.DisplayName = displayName;
            this.Value = enabled;
            this.CallingAssemblyName = $"{Assembly.GetCallingAssembly().GetName().Name}.{Assembly.GetCallingAssembly().GetType().GUID}";
            this.Shared = shared;
        }



        [JsonConstructor]
        private MenuBool()
        {
            
        }

        #endregion

        #region Public Properties

        internal override string Serialized => JsonConvert.SerializeObject(this, Formatting.Indented);

 
        /// <summary>
        ///     Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        [JsonProperty(Order = 3, PropertyName = "Value")]
        public new bool Value { get; set; }
        
        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Gets the render manager.
        /// </summary>
        /// <returns>Aimtec.SDK.Menu.Theme.IRenderManager.</returns>
        public override IRenderManager GetRenderManager()
        {
            return MenuManager.Instance.Theme.BuildMenuBoolRenderer(this);
        }

        /// <summary>
        ///     An application-defined function that processes messages sent to a window.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="wparam">Additional message information.</param>
        /// <param name="lparam">Additional message information.</param>
        public override void WndProc(uint message, uint wparam, int lparam)
        {
            if (message == (ulong) WindowsMessages.WM_LBUTTONUP && this.Visible)
            {
                var x = lparam & 0xffff;
                var y = lparam >> 16;

                if (MenuManager.Instance.Theme.GetMenuBoolControlBounds(this.Position, this.Parent.Width).Contains(x, y))
                {
                    this.UpdateValue(!this.Value);
                }
            }
        }



        #endregion

        #region Methods

        /// <summary>
        ///     Updates the value of the bool, saves the new value and fires the value changed event
        /// </summary>
        private void UpdateValue(bool newVal)
        {
            var oldClone = new MenuBool { InternalName = this.InternalName, DisplayName = this.DisplayName, Value = this.Value };

            this.Value = newVal;

            this.Save();

            this.FireOnValueChanged(this, new ValueChangedArgs(oldClone, this));
        }

        /// <summary>
        ///    Loads the value from the file for this component
        /// </summary>
        internal override void LoadValue()
        {
            if (File.Exists(this.ConfigPath))
            {
                var read = File.ReadAllText(this.ConfigPath);

                var sValue = JsonConvert.DeserializeObject<MenuBool>(read);

                if (sValue?.InternalName != null)
                {
                    this.Value = sValue.Value;
                }
            }
        }

        #endregion
    }
}