namespace Aimtec.SDK.Menu.Components
{
    using System;
    using System.IO;
    using System.Reflection;

    using Aimtec.SDK.Menu.Theme;

    using Newtonsoft.Json;

    using Util;

    /// <summary>
    ///     Class MenuList.
    /// </summary>
    /// <seealso cref="Aimtec.SDK.Menu.MenuComponent" />
    [JsonObject(MemberSerialization.OptIn)]
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
        /// <param name="shared">Whether this item is shared across instances</param>
        public MenuList(string internalName, string displayName, string[] items, int selectedValue, bool shared = false)
        {
            if (items.Length < selectedValue - 1)
            {
                throw new ArgumentException($"{nameof(selectedValue)} is outside the bounds of {nameof(items)}");
            }

            this.InternalName = internalName;
            this.DisplayName = displayName;
            this.Value = selectedValue;
            this.Items = items;
            this.Shared = shared;
            this.CallingAssemblyName = $"{Assembly.GetCallingAssembly().GetName().Name}.{Assembly.GetCallingAssembly().GetType().GUID}";

            this.LoadValue();
        }

        [JsonConstructor]
        private MenuList()
        {
            
        }

        #endregion

        #region Public Properties

        internal override string Serialized => JsonConvert.SerializeObject(this, Formatting.Indented);

        /// <summary>
        ///     Gets or sets the value, which is the index of the string list.
        /// </summary>
        /// <value>The value.</value>
        [JsonProperty(Order = 3)]
        public new int Value { get; set; }


        /// <summary>
        ///     Gets or sets the items.
        /// </summary>
        /// <value>The items.</value>
        [JsonProperty(Order = 4)]
        public string[] Items { get; set; }


        /// <summary>
        ///     Gets the selected item.
        /// </summary>
        /// <value>The selected item.</value>
        public string SelectedItem => this.Items[this.Value];

    
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
                        this.UpdateValue(this.Items.Length - 1);
                    }

                    else
                    {
                        this.UpdateValue(this.Value - 1);
                    }
                }

                else if (rightControl.Contains(x, y))
                {
                    if (this.Value == this.Items.Length - 1)
                    {
                        this.UpdateValue(0);
                    }

                    else
                    {
                        this.UpdateValue(this.Value + 1);
                    }
                }
            }
        }


        #endregion

        #region Methods

        /// <summary>
        ///     Updates the value of the MenuList, saves the new value and fires the value changed event
        /// </summary>
        private void UpdateValue(int newVal)
        {
            var oldClone = new MenuList { InternalName = this.InternalName, DisplayName = this.DisplayName, Items = this.Items, Value = this.Value };

            this.Value = newVal;

            this.SaveValue();

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

                var sValue = JsonConvert.DeserializeObject<MenuList>(read);

                if (sValue?.InternalName != null)
                {
                    this.Value = sValue.Value;
                }
            }
        }

        #endregion
    }
}