namespace Aimtec.SDK.Menu.Components
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Reflection;

    using Aimtec.SDK.Menu.Theme;
    using Aimtec.SDK.Menu.Theme.Default;

    using Newtonsoft.Json;

    using Util;

    /// <summary>
    ///     Class MenuSliderBool. This class cannot be inherited.
    /// </summary>
    /// <seealso cref="Aimtec.SDK.Menu.MenuComponent" />
    /// <seealso cref="int" />
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class MenuSliderBool : MenuComponent, IReturns<int>, IReturns<bool>
    {
        #region Constructors and Destructors

        /// <summary>
        ///  Initializes a new instance of the <see cref="MenuSliderBool" /> class.
        /// </summary>
        /// <param name="internalName">The internal name.</param>
        /// <param name="displayName">The display name.</param>
        /// <param name="value">The value.</param>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <param name="shared">Whether this item is shared across instances</param>
        public MenuSliderBool(string internalName, string displayName, bool enabled, int value, int minValue, int maxValue, bool shared = false)
        {
            this.InternalName = internalName;
            this.DisplayName = displayName;
            this.Enabled = enabled;
            this.Value = value;
            this.MinValue = minValue;
            this.MaxValue = maxValue;

            this.Shared = shared;

            this.CallingAssemblyName = $"{Assembly.GetCallingAssembly().GetName().Name}.{Assembly.GetCallingAssembly().GetType().GUID}";

            this.LoadValue();
        }

        [JsonConstructor]
        private MenuSliderBool()
        {
            
        }
        #endregion

        #region Public Properties

        internal override string Serialized => JsonConvert.SerializeObject(this, Formatting.Indented);


        [JsonProperty(Order = 3)]
        public new int Value { get; set; }

    

        /// <summary>
        ///     Gets or sets the maximum value.
        /// </summary>
        /// <value>The maximum value.</value>
        [JsonProperty(Order = 4)]
        public int MaxValue { get; set; }

        /// <summary>
        ///     Gets or sets the minimum value.
        /// </summary>
        /// <value>The minimum value.</value>
        [JsonProperty(Order = 5)]
        public int MinValue { get; set; }

        [JsonProperty(Order = 6)]
        public new bool Enabled { get; set; }


        /// <summary>
        ///     Gets or sets a value indicating whether [mouse down].
        /// </summary>
        /// <value><c>true</c> if [mouse down]; otherwise, <c>false</c>.</value>
        private bool MouseDown { get; set; }


        bool IReturns<bool>.Value
        {
            get => this.Enabled;
            set
            {
                this.Enabled = value;
            }
        }


        #endregion

        #region Public Methods and Operators

        public override Rectangle GetBounds(Vector2 pos)
        {
            var bounds = MenuManager.Instance.Theme.GetMenuSliderBoolControlBounds(pos);
            return Rectangle.Union(bounds[0], bounds[1]);
        }

        public override IRenderManager GetRenderManager()
        {
            return MenuManager.Instance.Theme.BuildMenuSliderBoolRenderer(this);
        }


        /// <summary>
        ///     An application-defined function that processes messages sent to a window.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="wparam">Additional message information.</param>
        /// <param name="lparam">Additional message information.</param>
        public override void WndProc(uint message, uint wparam, int lparam)
        {
            if ((message == (ulong)WindowsMessages.WM_LBUTTONDOWN
                || message == (ulong)WindowsMessages.WM_MOUSEMOVE && this.MouseDown) && this.Visible)
            {
                var x = lparam & 0xffff;
                var y = lparam >> 16;

                var bounds = MenuManager.Instance.Theme.GetMenuSliderBoolControlBounds(this.Position);
                var sliderBounds = bounds[0];

                if (sliderBounds.Contains(x, y))
                {
                    this.SetSliderValue(x);
                    this.MouseDown = true;
                }
            }

            else if (message == (ulong)WindowsMessages.WM_LBUTTONUP)
            {
                if (this.Visible && !this.MouseDown)
                {
                    var x = lparam & 0xffff;
                    var y = lparam >> 16;
                    var boolBounds = MenuManager.Instance.Theme.GetMenuSliderBoolControlBounds(this.Position)[1];
                    if (boolBounds.Contains(x, y))
                    {
                        this.UpdateEnabled(!this.Enabled);
                    }
                }

                this.MouseDown = false;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Sets the slider value.
        /// </summary>
        /// <param name="x">The x.</param>
        private void SetSliderValue(int x)
        {
            var sliderbounds = MenuManager.Instance.Theme.GetMenuSliderBoolControlBounds(this.Position)[0];
            this.UpdateValue(Math.Max(this.MinValue, Math.Min(this.MaxValue, (int)((x - this.Position.X) / (sliderbounds.Width - DefaultMenuTheme.LineWidth * 2) * this.MaxValue))));
        }


        /// <summary>
        ///     Updates the value of the slider, saves the new value and fires the value changed event
        /// </summary>
        /// <param name="newVal">The new value to set it to.</param>
        private void UpdateValue(int newVal)
        {
            var oldClone = new MenuSliderBool { InternalName = this.InternalName, DisplayName = this.DisplayName, Enabled = this.Enabled, Value = this.Value, MinValue = this.MinValue, MaxValue = this.MaxValue };

            this.Value = newVal;

            this.SaveValue();

            this.FireOnValueChanged(this, new ValueChangedArgs(oldClone, this));
        }

        /// <summary>
        ///     Updates the value of the Enabled variable, saves the new value and fires the value changed event
        /// </summary>
        /// <param name="newVal">The new value to set it to.</param>
        private void UpdateEnabled(bool newVal)
        {
            var oldClone = new MenuSliderBool { InternalName = this.InternalName, DisplayName = this.DisplayName, Enabled = this.Enabled, Value = this.Value, MinValue = this.MinValue, MaxValue = this.MaxValue };

            this.Enabled = newVal;

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

                var sValue = JsonConvert.DeserializeObject<MenuSliderBool>(read);

                if (sValue?.InternalName != null)
                {
                    this.Value = sValue.Value;
                    this.MaxValue = sValue.MaxValue;
                    this.MinValue = sValue.MinValue;
                    this.Enabled = sValue.Enabled;
                }
            }
        }

        #endregion
    }
}