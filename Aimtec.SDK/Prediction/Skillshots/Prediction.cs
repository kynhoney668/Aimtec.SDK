using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aimtec.SDK.Prediction.Skillshots
{
    /// <summary>
    /// Class Prediction.
    /// </summary>
    /// <seealso cref="Aimtec.SDK.Prediction.Skillshots.IPrediction" />
    public class Prediction : IPrediction
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static Prediction Instance { get; } = new Prediction();

        /// <summary>
        /// Gets or sets the implementation.
        /// </summary>
        /// <value>The implementation.</value>
        public static IPrediction Implementation { get; set; } = new PredictionImpl();

        /// <summary>
        /// Resets the implementation.
        /// </summary>
        public static void ResetImplementation()
        {
            Implementation = new PredictionImpl();
        }

        /// <summary>
        /// Calculates the target position.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="delay">The delay.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="speed">The speed.</param>
        /// <param name="from">From.</param>
        /// <param name="type">The type.</param>
        /// <returns>PredictedTargetPosition.</returns>
        public PredictedTargetPosition CalculateTargetPosition(
            Obj_AI_Base target,
            float delay,
            float radius,
            float speed,
            Vector3 from,
            SkillType type)
        {
            return Implementation.CalculateTargetPosition(target, delay, radius, speed, from, type);
        }

        /// <summary>
        /// Gets the prediction.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>PredictionOutput.</returns>
        public PredictionOutput GetPrediction(PredictionInput input)
        {
            return Implementation.GetPrediction(input);
        }
    }
}
