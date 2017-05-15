namespace Aimtec.SDK.Menu.Components
{
    using System;
    using System.Drawing;

    using Aimtec.SDK.Menu.Theme;
    using Aimtec.SDK.Menu.Theme.Default;

    using Util;

    // todo
    public sealed class MenuSliderBool : MenuComponent, IReturns<int>, IReturns<bool>
    {
        #region Constructors and Destructors

        public MenuSliderBool(string internalName, string displayName, bool enabled, int value, int minValue, int maxValue)
        {
            this.InternalName = internalName;
            this.DisplayName = displayName;
            this.Enabled = enabled;
            this.Value = value;
            this.MinValue = minValue;
            this.MaxValue = maxValue;
        }

        #endregion

        #region Public Properties

        public override string DisplayName { get; set; }

        public new bool Enabled { get; set; }

        public int MaxValue { get; set; }

        public int MinValue { get; set; }

        public override Vector2 Position { get; set; }

        public override bool Toggled { get; set; }

        public override bool Visible { get; set; }

        public override Menu Parent { get; set; }

        public override bool Root { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [mouse down].
        /// </summary>
        /// <value><c>true</c> if [mouse down]; otherwise, <c>false</c>.</value>
        private bool MouseDown { get; set; }

        public new int Value { get; set; }

        bool IReturns<bool>.Value
        {
            get => Enabled;
            set
            {
                Enabled = value;
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
                if (Visible && !this.MouseDown)
                {
                    var x = lparam & 0xffff;
                    var y = lparam >> 16;
                    var boolBounds = MenuManager.Instance.Theme.GetMenuSliderBoolControlBounds(this.Position)[1];
                    if (boolBounds.Contains(x, y))
                    {
                        this.Enabled = !this.Enabled;
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
            this.Value = Math.Max(this.MinValue, Math.Min(this.MaxValue, (int)((x - this.Position.X) / (sliderbounds.Width - DefaultMenuTheme.LineWidth * 2) * this.MaxValue)));
        }

        #endregion
    }
}