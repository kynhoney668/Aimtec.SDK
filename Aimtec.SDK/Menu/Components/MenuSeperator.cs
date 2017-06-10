namespace Aimtec.SDK.Menu.Components
{
    using Aimtec.SDK.Menu.Theme;

    using Newtonsoft.Json;

    [JsonObject(MemberSerialization.OptIn)]
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

        internal override string Serialized => JsonConvert.SerializeObject(this, Formatting.Indented);

        internal override bool SavableMenuItem { get; set; } = false;

        public string Value { get; set; }

        public string StringValue => Value;

        #endregion

        #region Public Methods and Operators

        public override IRenderManager GetRenderManager()
        {
            return MenuManager.Instance.Theme.BuildMenuSeperatorRenderer(this);
        }

        internal override void LoadValue()
        {
        }

        #endregion
    }
}