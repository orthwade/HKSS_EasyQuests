using HarmonyLib;
using System;

namespace owd.EasyQuests.HarmonyPatches
{
    [HarmonyPatch(typeof(EnemyJournalManager), nameof(EnemyJournalManager.RecordKill),
        new[] { typeof(EnemyJournalRecord), typeof(bool), typeof(bool) })]
    public static class EnemyJournalManager_RecordKill_Patch
    {
        // Prevent infinite recursion when we call RecordKill from within the patch.
        private static bool suppressPatch = false;

        private static void Postfix(EnemyJournalManager __instance, EnemyJournalRecord journalRecord, bool showPopup, bool forcePopup)
        {
            if (__instance == null)
            {
                PluginLogger.LogWarning("EnemyJournalManager_RecordKill_Patch: instance is null. Skipping patch.");
                return;
            }

            if (suppressPatch)
                return;

            Conf.QuestModifyMode mode = Conf.GetMode();

            int amount = 0;

            switch (mode)
            {
                case Conf.QuestModifyMode.Disabled:
                case Conf.QuestModifyMode.AutoComplete:
                    PluginLogger.LogInfo("EnemyJournalManager_RecordKill_Patch: Mod disabled or AutoComplete mode. Skip patch.");
                    return;

                case Conf.QuestModifyMode.OnlyOnePickRequired:
                    amount = GatherOrHuntQuest.GetTargetAmount(__instance.name);
                    break;

                case Conf.QuestModifyMode.Multiplier:
                    if(GatherOrHuntQuest.GetTargetAmount(__instance.name) > 0)
                        amount = 1 * Conf.GetMult();
                    break;
            }

            if (amount > 1)
            {
                PluginLogger.LogInfo($"EnemyJournalManager_RecordKill_Patch: Performing {amount - 1} additional RecordKill calls.");
                suppressPatch = true;
                try
                {
                    // Already recorded one kill from the original call, so do the rest.
                    for (int i = 1; i < amount; i++)
                        EnemyJournalManager.RecordKill(journalRecord, showPopup, forcePopup);
                }
                catch (Exception ex)
                {
                    PluginLogger.LogError($"EnemyJournalManager_RecordKill_Patch error: {ex}");
                }
                finally
                {
                    suppressPatch = false;
                }
            }
        }
    }
}
