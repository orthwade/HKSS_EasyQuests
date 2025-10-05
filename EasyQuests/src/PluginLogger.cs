using BepInEx.Logging;
using BepInEx.Configuration;

namespace owd.EasyQuests
{
    /// <summary>
    /// Centralized logger for the Tools Melee Recharge plugin.
    /// </summary>
    public static class PluginLogger
    {
        // Logging is enabled by default
        public static bool enableLogging = true;
        private static ManualLogSource logger;

        public static void Init(ConfigFile config)
        {
            if (logger == null)
            {
                logger = BepInEx.Logging.Logger.CreateLogSource("EasyQuests");
                LoggingInit(config);
            }

        }

        public static void LogInfo(string message)
        {
            if (!enableLogging) return;
            logger.LogInfo(message);
        }

        public static void LogWarning(string message)
        {
            if (!enableLogging) return;
            logger.LogWarning(message);
        }

        public static void LogError(string message)
        {
            if (!enableLogging) return;
            logger.LogError(message);
        }

        private static ConfigEntry<bool> LoggingEnabled;
        private static void LoggingInit(ConfigFile config)
        {
            LoggingEnabled = config.Bind(
                "00 -- General",              // Section
                "Logging",       // Key
                true,                   // Default value
                 new ConfigDescription(
                    "Enable or disable logging to BepInEx log file",
                    null,
                    new ConfigurationManagerAttributes { Order = -1 }
                )
            );

            enableLogging = LoggingEnabled.Value;

            LoggingEnabled.SettingChanged += (sender, args) =>
            {
                enableLogging = LoggingEnabled.Value;
            };
        }
    }
}
