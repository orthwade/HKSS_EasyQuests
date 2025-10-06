using System.Collections.Generic;
using BepInEx.Configuration;

namespace owd.EasyQuests
{
    public class QuestSpecificConf
    {
        public enum QuestSpecificModifyMode{ Global, AutoComplete, OnlyOnePickRequired, Multiplier, Disabled };
        
        public List<QuestData> listQuestData { get; private set; }

        private ConfigEntry<QuestSpecificModifyMode> modifyModeEntry;
        public QuestSpecificModifyMode GetMode() { return modifyModeEntry.Value; }
        private ConfigEntry<int> multiplierEntry;
        public int GetMult() { return multiplierEntry.Value; }

        public QuestSpecificConf(ConfigFile config, int n, List<QuestData> _listQuestData)
        {
            listQuestData = _listQuestData;

            modifyModeEntry = config.Bind(
                $"{n:D2} -- {_listQuestData[0].DisplayName}",
                $"QuestModifyMode",
                QuestSpecificModifyMode.Global,
                 new ConfigDescription(
                    "",
                    null,
                    new ConfigurationManagerAttributes { Order = -1 }
                )
            );
            modifyModeEntry.SettingChanged += (_, __) =>
            {
                CheckAndApplyAutoComplete.Do();
            };
            multiplierEntry = config.Bind(
                $"{n:D2} -- {_listQuestData[0].DisplayName}",
                "Multiplier",
                2,
                new ConfigDescription(
                    "Multiplies loot amount.\n. Only works if QuestModifyMode is Multiplier",
                    new AcceptableValueRange<int>(1, 35),
                    new ConfigurationManagerAttributes { Order = -2 }
                )
            );
        }
    }
}