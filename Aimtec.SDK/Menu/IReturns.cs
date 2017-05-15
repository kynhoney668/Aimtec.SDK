namespace Aimtec.SDK.Menu
{
    /// <summary>
    ///     Exposes a property of type <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IEnabledReturn
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets the value.
        /// </summary>
        /// <value>
        ///     The value.
        /// </value>
        bool Enabled { get; }

        #endregion
    }

    /// <summary>
    ///     Exposes a property of type <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IIntReturn
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets the value.
        /// </summary>
        /// <value>
        ///     The value.
        /// </value>
        int Value { get; }

        #endregion
    }



    /// <summary>
    ///     Exposes a property of type <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IStringReturn
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets the value.
        /// </summary>
        /// <value>
        ///     The value.
        /// </value>
        string StringValue { get; }

        #endregion
    }


}