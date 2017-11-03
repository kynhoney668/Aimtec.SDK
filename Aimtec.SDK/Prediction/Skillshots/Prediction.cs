namespace Aimtec.SDK.Prediction.Skillshots
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    using Aimtec.SDK.Menu;
    using Aimtec.SDK.Menu.Components;
    using Aimtec.SDK.Menu.Config;

    public class Prediction : ISkillshotPrediction
    {
        private Menu PredConfig;

        private MenuList PredictionMenuItem;

        private Dictionary<string, ISkillshotPrediction> Implementations = new Dictionary<string, ISkillshotPrediction>();

        #region Constructors and Destructors

        private Prediction()
        {
            this.PredConfig = new Menu("Prediction", "Prediction");

            this.PredConfig.Add(new MenuBool("displayPred", "Display Pred Type"));

            this.Implementations["Default"] = new PredictionImpl();
            this.Implementations["PredImplB"] = new PredictionImplB();

            this.PredictionMenuItem = new MenuList("PredictionType", "Prediction", this.Implementations.Keys.ToArray(), 1);
            this.PredConfig.Add(this.PredictionMenuItem);

            AimtecMenu.Instance.Add(this.PredConfig);

            this.PredictionMenuItem.OnValueChanged += (sender, args) =>
                {
                    var selectedPred = args.GetNewValue<MenuList>().SelectedItem;
                    this.Implementation = this.Implementations[selectedPred];
                    Console.WriteLine($"Changed Prediction Implementation to {selectedPred}");
                };

            this.Implementation = this.Implementations[this.PredictionMenuItem.SelectedItem];

            Render.OnPresent += this.RenderOnOnPresent;
        }

        private void RenderOnOnPresent()
        {
            if (this.PredConfig["displayPred"].Enabled)
            {
                Render.Text($"Prediction: {this.PredictionMenuItem.SelectedItem}",  new Vector2(0.10f * Render.Width, 0.10f* Render.Height), RenderTextFlags.Center, Color.AliceBlue);
            }
        }

        #endregion

        #region Public Properties

        public static Prediction Instance { get; } = new Prediction();

        public ISkillshotPrediction Implementation { get; private set; }

        #endregion

        #region Public Methods and Operators

        public void AddPredictionImplementation(string name, ISkillshotPrediction pred)
        {
            this.Implementations[name] = pred;
            this.UpdatePredictionImplementations();
        }


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

        #endregion

        private void UpdatePredictionImplementations()
        {
            string[] updatedPredList = this.Implementations.Keys.ToArray();
            this.PredictionMenuItem.Items = updatedPredList;
            this.Implementation = this.Implementations[this.PredictionMenuItem.SelectedItem];
        }
    }
}