using HarmonyLib;
using System;

namespace owd.EasyQuests.HarmonyPatches
{
    [HarmonyPatch(typeof(CollectableItemManager), nameof(CollectableItemManager.AddItem),
        new[] { typeof(CollectableItem), typeof(int) })]
    public static class CollectableItemManager_AddItem_Patch
    {
        private static bool Prefix(CollectableItem item, ref int amount)
        {
            if (Conf.GetMode() == Conf.QuestModifyMode.Disabled)
            {
                PluginLogger.LogWarning("CollectableItemManager_AddItem_Patch Prefix: mod is disabled. Skip patch");
                return true;
            }
            PluginLogger.LogInfo("CollectableItemManager_AddItem_Patch Prefix");

            if (item == null)
            {
                PluginLogger.LogWarning("CollectableItemManager_AddItem_Patch Prefix: __item == null");

                return true;
            }

            PluginLogger.LogInfo($"CollectableItemManager_AddItem_Patch_Prefix. Item name: {item.name}");

            PluginLogger.LogInfo($"Amount before to add before modifications: {amount}");

            if (amount > 0)
            {
                Conf.QuestModifyMode mode = Conf.GetMode();

                if (mode == Conf.QuestModifyMode.AutoComplete)
                {
                    PluginLogger.LogWarning("CollectableItemManager_AddItem_Patch. Using mode AutoComplete. Skip patch.");
                    return true;
                }

                string key = item.name;

                if (key == null)
                    return true;

                int val_1 = GatherOrHuntQuest.GetTargetAmount(key);
                if (val_1 < 1)
                {
                    PluginLogger.LogWarning("CollectableItemManager_AddItem_Patch. item is not in QuestDatabase");
                    return true;
                }
                int val_0 = item.CollectedAmount;

                int toTarget = val_1 - val_0;

                if (mode == Conf.QuestModifyMode.Multiplier)
                {
                    PluginLogger.LogInfo("CollectableItemManager_AddItem_Patch. Using mode Multiplier");
                    amount *= Conf.GetMult();
                    amount = Math.Min(amount, toTarget);
                }
                else if (mode == Conf.QuestModifyMode.OnlyOnePickRequired)
                {
                    PluginLogger.LogInfo("CollectableItemManager_AddItem_Patch. Using mode OnlyOnePickRequired");

                    amount = toTarget;
                }
                else
                {
                    PluginLogger.LogError("CollectableItemManager_AddItem_Patch. Something is terribly wrong.");
                    return true;
                }

                PluginLogger.LogInfo("Patching CollectableItemManager_AddItem amount\n" +
                $"Collectable Amount before add: {val_0}\n" +
                $"Collectable Amount Target: {val_1}\n" +
                $"New add amount(amount var): {amount}");
            }

            return true;
        }
    }
}
