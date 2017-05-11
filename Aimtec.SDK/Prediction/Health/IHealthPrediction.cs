using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aimtec.SDK.Prediction.Health
{
    /// <summary>
    /// Interface IHealthPrediction
    /// </summary>
    public interface IHealthPrediction
    {
        /// <summary>
        /// Gets the predicted health of the target.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="time">The time.</param>
        /// <returns>System.Single.</returns>
        float GetPrediction(Obj_AI_Base target, int time);
    }
}
