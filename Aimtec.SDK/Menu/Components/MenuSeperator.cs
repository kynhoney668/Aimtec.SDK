namespace Aimtec.SDK.Menu.Components
{
    using System;
    using Aimtec.SDK.Menu.Theme;

    public sealed class MenuSeperator : MenuComponent, IReturns<string>
    {
        #region Constructors and Destructors

        public MenuSeperator(string internalName, string text = "")
        {
            this.InternalName = internalName;
            this.DisplayName = text;
            this.Value = text;
        }

        #endregion

        #region Public Properties

        public override string DisplayName { get; set; }

        public override Vector2 Position { get; set; }

        public override bool Toggled { get; set; }

        public string Value { get; set; }

        public override bool Visible { get; set; }

        public override Menu Parent { get; set; }

        public override bool Root { get; set; }

        public string StringValue => Value;

        #endregion

        #region Public Methods and Operators

        public override IRenderManager GetRenderManager()
        {
            return MenuManager.Instance.Theme.BuildMenuSeperatorRenderer(this);
        }

        #endregion
    }
}