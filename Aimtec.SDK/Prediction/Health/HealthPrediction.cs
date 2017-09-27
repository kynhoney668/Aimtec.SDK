namespace Aimtec.SDK.Prediction.Health
{
    using System;

    /// <inheritdoc />
    public class HealthPrediction : IHealthPrediction
    {
        #region Public Properties

        /// <summary>
        ///     Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static HealthPrediction Instance { get; } = new HealthPrediction();

        /// <summary>
        ///     Gets or sets the implementation.
        /// </summary>
        /// <value>The implementation.</value>
        [Obsolete("This is obsolete. Use HealthPrediction.Instance instead. ")]
        public static IHealthPrediction Implementation { get; set; } = new HealthPredictionImplB();

        #endregion

        #region Public Methods and Operators

        /// <inheritdoc />
        public float GetLaneClearHealthPrediction(Obj_AI_Base target, int time)
        {
            return Implementation.GetLaneClearHealthPrediction(target, time);
        }

        /// <inheritdoc />
        public float GetPrediction(Obj_AI_Base target, int time)
        {
            return Implementation.GetPrediction(target, time);
        }

        #endregion
    }
}
