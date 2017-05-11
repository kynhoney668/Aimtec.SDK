namespace Aimtec.SDK.Menu.Components
{
    using System;
    using System.Drawing;

    using Aimtec.SDK.Menu.Theme;

    // todo
    public sealed class MenuSliderBool : MenuComponent, IReturns<int>
    {
        #region Constructors and Destructors

        public MenuSliderBool(string displayName, bool enabled, int value, int minValue, int maxValue)
        {
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

        public int Value { get; set; }

        public override bool Visible { get; set; }

        #endregion

        #region Public Methods and Operators

        public override Rectangle GetBounds(Vector2 pos)
        {
            return Rectangle.Union(this.GetSliderBounds(pos), this.GetBoolBounds(pos));
        }

        public override IRenderManager GetRenderManager()
        {
            throw new NotImplementedException();
        }

        public override void Render(Vector2 pos)
        {
            throw new NotImplementedException();
        }

        public override void WndProc(uint message, uint wparam, int lparam)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Methods

        private Rectangle GetBoolBounds(Vector2 pos)
        {
            return default(Rectangle);
        }

        private Rectangle GetSliderBounds(Vector2 pos)
        {
            return default(Rectangle);
        }

        #endregion
    }
}