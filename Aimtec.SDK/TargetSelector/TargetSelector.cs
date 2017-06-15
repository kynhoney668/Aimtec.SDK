namespace Aimtec.SDK.TargetSelector
{
    using Aimtec.SDK.Menu;
    using Aimtec.SDK.Menu.Config;

    using NLog;
    using NLog.Fluent;
    using System.Linq;

    /// <summary>
    ///     Target Selector Class
    /// </summary>
    public class TargetSelector
    {
        private static Menu TargetSelectorMenu { get; set; }

        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        internal static void Load()
        {
            Log.Info().Message("Loading Default Target Selector").Write();
            Implementation = new TargetSelectorImpl();
        }

        internal static void LoadMenu()
        {
            Logger.Info("Loading the Target Selector Menu");
   
            TargetSelectorMenu = impl.Config;

            AimtecMenu.Instance.Add(TargetSelectorMenu);
        }


        /// <summary>
        ///     Initializes a new instance of the <see cref="TargetSelector" /> class.
        /// </summary>
        /// <param name="impl">The implementation.</param>
        public TargetSelector(ITargetSelector impl)
        {
            Implementation = impl;
        }

        /// <summary>
        ///     Gets or sets the implementation of the target selector.
        /// </summary>
        /// <value>
        ///     The implementation of the target selector.
        /// </value>
        public static ITargetSelector Implementation
        {
            get => impl;
            set
            {
                if (impl == null)
                {
                    Logger.Info("Target Selector Implementation is being initialized.");
                }

                else
                {
                    Logger.Info("Target Selector Implementation is being changed.");
                    Dispose();
                }

                impl = value;

                LoadMenu();
            }
        }

        private static ITargetSelector impl;

        /// <summary>
        ///     Gets the target.
        /// </summary>
        /// <param name="range">The range.</param>
        /// <returns></returns>
        public static Obj_AI_Hero GetTarget(float range)
        {
            return Implementation.GetTarget(range);
        }

        /// <summary>
        ///     Gets the best target within Auto Attack Range
        /// </summary>
        public static Obj_AI_Hero GetOrbwalkingTarget()
        {
            return Implementation.GetOrbwalkingTarget();
        }

        /// <summary>
        ///     Gets an ordered enumerable of the available targets within the range based on their priority ranking by the Target Selector
        /// </summary>
        public IOrderedEnumerable<Obj_AI_Hero> GetOrderedTargets(float range)
        {
            return Implementation.GetOrderedTargets(range);
        }
            

        /// <summary>
        ///     Disposes the current instance
        /// </summary>
        public static void Dispose()
        {
            Logger.Info("Disposing the old Implementation");

            if (TargetSelectorMenu != null)
            {
                TargetSelectorMenu.Dispose();
            }

            if (Implementation != null)
            {
                Implementation.Dispose();
            }
        }
    }
}