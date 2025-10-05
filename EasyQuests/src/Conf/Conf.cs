using BepInEx.Configuration;

namespace owd.EasyQuests
{
    internal static class Conf
    {
        private static ConfigEntry<bool> enabledEntry;
        public static bool IsModEnabled() { return enabledEntry.Value; }
        public enum QuestModifyMode { AutoComplete, OnlyOnePickRequired, Multiplier };

        private static ConfigEntry<QuestModifyMode> questModifyModeEntry;
        public static QuestModifyMode GetMode() { return questModifyModeEntry.Value; }
        private static ConfigEntry<int> multiplierEntry;
        public static int GetMult() { return multiplierEntry.Value; }

        public static void Init(ConfigFile config)
        {
            enabledEntry = config.Bind(
                "00 -- General",
                "Enabled",
                false,
                 new ConfigDescription(
                    "Is mod enabled",
                    null,
                    new ConfigurationManagerAttributes { Order = -1 }
                )
            );
            questModifyModeEntry = config.Bind(
                "00 -- General",
                "QuestModifyMode",
                QuestModifyMode.AutoComplete,
                 new ConfigDescription(
                    "Quest gets completed right when it starts",
                    null,
                    new ConfigurationManagerAttributes { Order = -2 }
                )
            );
            multiplierEntry = config.Bind(
                "00 -- General",
                "Multiplier",
                2,
                new ConfigDescription(
                    "Multiplies loot amount.\n. Only works if QuestModifyMode is Multiplier",
                    new AcceptableValueRange<int>(1, 35),
                    new ConfigurationManagerAttributes { Order = -4 }
                )
            );
        }
       
    }
}
