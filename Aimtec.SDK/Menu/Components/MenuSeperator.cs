namespace Aimtec.SDK.Menu.Components
{
    using Aimtec.SDK.Menu.Theme;

    public sealed class MenuSeperator : MenuComponent, IReturns<string>
    {
        #region Constructors and Destructors

        public MenuSeperator(string internalName, string displayName, string text = "")
        {
            this.InternalName = internalName;
            this.DisplayName = displayName;
            this.Value = text;
        }

        #endregion

        #region Public Properties

        public override string DisplayName { get; set; }

        public override Vector2 Position { get; set; }

        public override bool Toggled { get; set; }

        public string Value { get; set; }

        public override bool Visible { get; set; }

        #endregion

        #region Public Methods and Operators

        public override IRenderManager GetRenderManager()
        {
            return MenuManager.Instance.Theme.BuildMenuSeperatorRenderer(this);
        }

        #endregion
    }
}