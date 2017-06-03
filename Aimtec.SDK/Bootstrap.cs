namespace Aimtec.SDK
{
    using System;
    using System.IO;
    using System.Reflection;

    using NLog;
    using NLog.Config;
    using NLog.Layouts;
    using NLog.Targets;
    using NLog.Targets.Wrappers;

    /// <summary>
    ///     Class Bootstrap.
    /// </summary>
    public class Bootstrap // : ILibraryEntryPoint
    {
        #region Static Fields

        /// <summary>
        ///     Gets if the library has already been loaded and initalized
        /// </summary>
        private static bool alreadyLoaded;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Initalizes the library.
        /// </summary>
        public void Load()
        {
            if (alreadyLoaded)
            {
                return;
            }

            SetupLogging();

            Logger.Info($"Aimtec.SDK version {Assembly.GetExecutingAssembly().GetName().Version} loaded.");

            alreadyLoaded = true;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Setups the logging.
        /// </summary>
        private static void SetupLogging()
        {
            // Setup NLog with async console and file logging.
            // Only logs to file if the log level is greater or equal to the warn level.
            var config = new LoggingConfiguration();

            var consoleTarget = new AsyncTargetWrapper(
                new ColoredConsoleTarget("ColoredConsoleTarget")
                {
                    Layout = new SimpleLayout("${longdate}|${pad:padding=5:inner=${level:uppercase=true}}|${message}")
                });

            config.AddTarget(consoleTarget);
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, consoleTarget);

            var asyncFileTarget = new AsyncTargetWrapper(
                new FileTarget("FileTarget")
                {
                    Layout = new SimpleLayout("${longdate}|${pad:padding=5:inner=${level:uppercase=true}}|${message}"),
                    LineEnding = LineEndingMode.Default,
                    DeleteOldFileOnStartup = true,
                    FileName = new SimpleLayout(
                        Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                            "aimtec.loader",
                            "GameLogs",
                            "Aimtec.SDK.log")),
                    OpenFileCacheTimeout = 30,
                    KeepFileOpen = true
                });

            config.AddTarget(asyncFileTarget);
            config.AddRule(LogLevel.Warn, LogLevel.Fatal, asyncFileTarget);

            LogManager.Configuration = config;
        }

        #endregion
    }
}