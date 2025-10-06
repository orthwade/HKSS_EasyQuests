namespace owd.EasyQuests
{
    public static class CheckAndApplyAutoComplete
    {
        public static void Do()
        {
            if (HeroController.instance == null)
            {
                return;
            }
            
            if (Conf.GetMode() == Conf.QuestModifyMode.AutoComplete)
            {
                PluginLogger.LogInfo("HeroController_Start_Patch Postfix.\nMode AutoComplete. Attempting");

                var list = GatherOrHuntQuest.GetListQuestsActiveAndNotCompleteAndNotAtTargetCount();

                foreach (var (questData, fullQuestBase) in list)
                {
                    foreach (QuestTargetData questTargetData in questData.Targets)
                    {
                        if (typeof(CollectableItem).IsAssignableFrom(questTargetData.CounterClassType))
                        {
                            var items = CollectableItemManager.Instance.GetAllCollectables();

                            foreach (CollectableItem item in items)
                            {
                                if (item != null && item.name == questTargetData.CounterName)
                                {
                                    int amount = questTargetData.Amount - item.CollectedAmount;

                                    if (amount > 0)
                                    {
                                        PluginLogger.LogInfo("HeroController_Start_Patch Postfix.\n Attempting AddItem");

                                        CollectableItemManager.AddItem(item, amount);
                                    }
                                    break;
                                }
                            }
                        }
                        else if (typeof(EnemyJournalRecord).IsAssignableFrom(questTargetData.CounterClassType))
                        {
                            var record = EnemyJournalManager.GetRecord(questTargetData.CounterName);

                            int amount = questTargetData.Amount - record.KillCount;

                            if (amount > 0)
                            {
                                PluginLogger.LogInfo("HeroController_Start_Patch Postfix.\n Attempting RecordKill");

                                for (int i = 0; i < amount; ++i)
                                {
                                    EnemyJournalManager.RecordKill(record, false, false);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}