using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace owd.EasyQuests.Patches
{
    [HarmonyPatch(typeof(FullQuestBase),
        nameof(FullQuestBase.TryEndQuest),
        new[] { typeof(Action), typeof(bool), typeof(bool), typeof(bool) })]
    internal static class FullQuestBase_TryEndQuest_Patch
    {
        static void Prefix()
        {
            // Capture stack trace for debug
            var stackTrace = new StackTrace(true);
            var frames = stackTrace.GetFrames();
            if (frames == null) return;

            var sb = new StringBuilder();
            sb.AppendLine("=== TryEndQuest called ===");

            foreach (var frame in frames)
            {
                var method = frame.GetMethod();
                sb.AppendLine(string.Format("{0}.{1} (at {2}:{3})",
                    method?.DeclaringType?.FullName,
                    method?.Name,
                    frame.GetFileName(),
                    frame.GetFileLineNumber()));
            }

            PluginLogger.LogInfo("FullQuestBase_TryEndQuest_Patch. Stack:\n" + sb);
        }
    }

    [HarmonyPatch(typeof(FullQuestBase),
        nameof(FullQuestBase.BeginQuest),
        new[] { typeof(Action), typeof(bool) })]
    internal static class FullQuestBase_BeginQuest_Patch
    {
        private static void AddCollectable(CollectableItem item, int amount)
        {
            PluginLogger.LogInfo(string.Format(
                "FullQuestBase_BeginQuest_Patch: Adding CollectableItem '{0}' x{1}",
                item.name, amount));

            for (int i = item.CollectedAmount; i < amount; i++)
            {
                CollectableItemManager.AddItem(item, 1);
            }
        }

        private static void AddEnemyRecord(EnemyJournalRecord record, int amount)
        {
            PluginLogger.LogInfo(string.Format(
                "FullQuestBase_BeginQuest_Patch: Recording Enemy '{0}' x{1}",
                record.name, amount));

            for (int i = 0; i < amount; i++)
            {
                EnemyJournalManager.RecordKill(record, false, false);
            }
        }

        private static void CompleteQuestImmediately(FullQuestBase instance)
        {
            if (instance == null || instance.Targets == null)
            {
                PluginLogger.LogWarning("CompleteQuestImmediately: instance or Targets is null.");
                return;
            }

            PluginLogger.LogInfo(string.Format("Completing quest '{0}' synchronously.", instance.name));

            foreach (var target in instance.Targets)
            {
                if (target.Counter == null)
                {
                    PluginLogger.LogWarning("CompleteQuestImmediately: Target.Counter is null.");
                    continue;
                }

                int count = target.Count;

                for (int i = 0; i < count; i++)
                {
                    var data = GameManager.instance.playerData.QuestCompletionData;
                    var completion = data.GetData(instance.name);
                    completion.CompletedCount++;
                    data.SetData(instance.name, completion);
                }

                int total = GameManager.instance.playerData.QuestCompletionData.GetData(instance.name).CompletedCount;
                PluginLogger.LogInfo(string.Format("Quest '{0}' progress: {1}/{2}", instance.name, total, count));
            }

            PluginLogger.LogInfo(string.Format("Quest '{0}' fully completed.", instance.name));
        }

        private static void Postfix(FullQuestBase __instance)
        {
            if (__instance == null)
            {
                PluginLogger.LogWarning("FullQuestBase_BeginQuest_Patch: __instance is null. Skipping.");
                return;
            }

            var mode = Conf.GetMode();

            if (mode == Conf.QuestModifyMode.Disabled)
            {
                PluginLogger.LogInfo("FullQuestBase_BeginQuest_Patch: mod disabled. Skipping.");
                return;
            }

            if (mode != Conf.QuestModifyMode.AutoComplete)
            {
                PluginLogger.LogInfo("FullQuestBase_BeginQuest_Patch: mode not AutoComplete. Skipping.");
                return;
            }

            PluginLogger.LogInfo(string.Format(
                "FullQuestBase_BeginQuest_Patch: AutoComplete active for quest '{0}'", __instance.name));

            var manager = CollectableItemManager.Instance;
            if (manager == null)
            {
                PluginLogger.LogWarning("CollectableItemManager.Instance is null. Skipping.");
                return;
            }

            Quest quest = __instance as Quest;
            if (quest == null)
            {
                PluginLogger.LogWarning("FullQuestBase_BeginQuest_Patch: instance is not a Quest. Skipping.");
                return;
            }

            if (quest.QuestType == null ||
                !(quest.QuestType.name == "Gather" || quest.QuestType.name == "Hunt"))
            {
                PluginLogger.LogInfo(string.Format(
                    "FullQuestBase_BeginQuest_Patch: Quest type '{0}' not supported. Skipping.",
                    quest.QuestType != null ? quest.QuestType.name : "Unknown"));
                return;
            }

            var targets = __instance.Targets;
            if (targets == null || targets.Count == 0)
            {
                PluginLogger.LogWarning(string.Format(
                    "FullQuestBase_BeginQuest_Patch: Quest '{0}' has no targets.", quest.name));
                return;
            }

            PluginLogger.LogInfo(string.Format(
                "FullQuestBase_BeginQuest_Patch: Completing {0} targets synchronously.", targets.Count));

            foreach (var target in targets)
            {
                var counter = target.Counter;
                if (counter == null)
                {
                    PluginLogger.LogWarning("FullQuestBase_BeginQuest_Patch: Target.Counter is null. Skipping target.");
                    continue;
                }

                int amount = target.Count;

                if (counter is CollectableItem)
                {
                    AddCollectable((CollectableItem)counter, amount);
                }
                else if (counter is EnemyJournalRecord)
                {
                    AddEnemyRecord((EnemyJournalRecord)counter, amount);
                }
                else
                {
                    PluginLogger.LogWarning(string.Format(
                        "FullQuestBase_BeginQuest_Patch: Counter '{0}' is neither CollectableItem nor EnemyJournalRecord.",
                        counter.name));
                }
            }

            CompleteQuestImmediately(__instance);
        }
    }
}
