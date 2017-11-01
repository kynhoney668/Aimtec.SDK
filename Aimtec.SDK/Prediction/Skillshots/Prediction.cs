namespace Aimtec.SDK.Prediction.Skillshots
{

    public class Prediction : ISkillshotPrediction
    {
        #region Constructors and Destructors

        private Prediction()
        {
            this.ResetImplementation();
        }

        #endregion

        #region Public Properties

        public static Prediction Instance { get; } = new Prediction();

        public ISkillshotPrediction Implementation { get; set; }

        #endregion

        #region Public Methods and Operators

        public PredictionOutput GetPrediction(PredictionInput input)
        {
            var output = this.Implementation.GetPrediction(input);
            output.Input = input;
            return output;
        }

        public PredictionOutput GetPrediction(PredictionInput input, bool ft, bool collision)
        {
            var output = this.Implementation.GetPrediction(input, ft, collision);
            output.Input = input;
            return output;
        }

        public void ResetImplementation()
        {
            this.Implementation = new PredictionImpl();
        }

        #endregion
    }
}