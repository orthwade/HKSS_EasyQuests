using System.Collections.Generic;
using BepInEx.Configuration;

namespace owd.EasyQuests
{
    public static class Conf
    {
        public enum QuestModifyMode { AutoComplete, OnlyOnePickRequired, Multiplier, Disabled };


        private static ConfigEntry<QuestModifyMode> questModifyModeEntry;
        public static QuestModifyMode GetMode() { return questModifyModeEntry.Value; }
        private static ConfigEntry<int> multiplierEntry;
        public static int GetMult() { return multiplierEntry.Value; }

        private static Dictionary<string, QuestSpecificConf> dictQuestSpecificConfByDisplayName =
        new Dictionary<string, QuestSpecificConf>();

        private static Dictionary<string, QuestSpecificConf> dictQuestSpecificConfByInternalName =
        new Dictionary<string, QuestSpecificConf>();

        public static void Init(ConfigFile config)
        {
            questModifyModeEntry = config.Bind(
                "00 -- General",
                "QuestModifyMode",
                QuestModifyMode.AutoComplete,
                 new ConfigDescription(
                    "",
                    null,
                    new ConfigurationManagerAttributes { Order = -2 }
                )
            );
            questModifyModeEntry.SettingChanged += (_, __) =>
            {
                CheckAndApplyAutoComplete.Do();
            };
            multiplierEntry = config.Bind(
                "00 -- General",
                "Multiplier",
                2,
                new ConfigDescription(
                    "Multiplies loot amount.\n. Only works if QuestModifyMode is Multiplier",
                    new AcceptableValueRange<int>(1, 35),
                    new ConfigurationManagerAttributes { Order = -3 }
                )
            );

            int i = 1;

            foreach (var pairKeyValue in QuestDatabase.QuestsByDisplayName)
            {
                string displayName = pairKeyValue.Key;
                List<QuestData> listQuestData = pairKeyValue.Value;

                QuestSpecificConf questSpecificConf = new QuestSpecificConf(config, i, listQuestData);
                dictQuestSpecificConfByDisplayName.Add(displayName, questSpecificConf);
                ++i;
            }
            foreach (var pairKeyValue in dictQuestSpecificConfByDisplayName)
            {
                string displayName = pairKeyValue.Key;
                QuestSpecificConf questSpecificConf = pairKeyValue.Value;
                List<QuestData> listQuestData = questSpecificConf.listQuestData;

                foreach (QuestData questData in listQuestData)
                {
                    dictQuestSpecificConfByInternalName.Add(questData.Name, questSpecificConf);
                }
            }
        }

        private static QuestModifyMode ResolveMode(QuestSpecificConf.QuestSpecificModifyMode questSpecificModifyMode)
        {
            switch (questSpecificModifyMode)
            {
                case QuestSpecificConf.QuestSpecificModifyMode.Global:
                    return GetMode();
                case QuestSpecificConf.QuestSpecificModifyMode.AutoComplete:
                    return QuestModifyMode.AutoComplete;
                case QuestSpecificConf.QuestSpecificModifyMode.OnlyOnePickRequired:
                    return QuestModifyMode.OnlyOnePickRequired;
                case QuestSpecificConf.QuestSpecificModifyMode.Multiplier:
                    return QuestModifyMode.Multiplier;
                case QuestSpecificConf.QuestSpecificModifyMode.Disabled:
                    return QuestModifyMode.Disabled;
            }

            return QuestModifyMode.Disabled;
        }


        public static QuestModifyMode ResolveModeByCounterName(string counterName)
        {
            if (string.IsNullOrEmpty(counterName) ||
                QuestDatabase.QuestsByCounterName == null ||
                !QuestDatabase.QuestsByCounterName.TryGetValue(counterName, out var listQuestData) ||
                listQuestData == null || listQuestData.Count == 0)
            {
                return QuestModifyMode.Disabled;
            }

            foreach (var questData in listQuestData)
            {
                if (questData == null || string.IsNullOrEmpty(questData.DisplayName))
                    continue;

                if (!dictQuestSpecificConfByDisplayName.TryGetValue(questData.DisplayName, out var questSpecificConf) ||
                    questSpecificConf == null)
                {
                    continue;
                }

                var modeSpecific = questSpecificConf.GetMode();
                return ResolveMode(modeSpecific); // returns on first found
            }

            return QuestModifyMode.Disabled;
        }


        public static QuestModifyMode ResolveModeByQuestInternalName(string questInternalName)
        {
            if (string.IsNullOrEmpty(questInternalName) ||
                dictQuestSpecificConfByInternalName == null ||
                !dictQuestSpecificConfByInternalName.TryGetValue(questInternalName, out var questSpecificConf) ||
                questSpecificConf == null)
            {
                return QuestModifyMode.Disabled;
            }

            try
            {
                var mode = questSpecificConf.GetMode();
                return ResolveMode(mode);
            }
            catch
            {
                // In case GetMode() throws for some reason
                return QuestModifyMode.Disabled;
            }
        }
    }
}
