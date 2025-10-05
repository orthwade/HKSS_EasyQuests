using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace owd.EasyQuests.Patches
{
    [HarmonyPatch(typeof(FullQuestBase),
    nameof(FullQuestBase.BeginQuest),
    new[] { typeof(Action), typeof(bool) })]
    internal static class FullQuestBase_BeginQuest_Patch
    {
        private static void Add(CollectableItem item, int amount)
        {
            PluginLogger.LogInfo("FullQuestBase_BeginQuest_Patch (TEST) Attempting add item\n" +
            $"Item Name: {item.name}\n" +
            $"Amount: {amount}\n");
            CollectableItemManager.AddItem(item, amount);
        }
        private static void Postfix(FullQuestBase __instance)
        {
            if (!Conf.IsModEnabled())
            {
                PluginLogger.LogWarning("FullQuestBase_BeginQuest_Patch Postfix: mod is disabled. Skip patch");
                return;
            }
            if (__instance == null)
            {
                PluginLogger.LogWarning("FullQuestBase instance is null in BeginQuest postfix");
                return;
            }

            PluginLogger.LogInfo("FullQuestBase_BeginQuest Postfix");

            if (Conf.GetMode() == Conf.QuestModifyMode.AutoComplete)
            {
                PluginLogger.LogInfo("FullQuestBase_BeginQuest_Patch mode is AutoComplete");

                CollectableItemManager manager = CollectableItemManager.Instance;

                if (manager == null)
                {
                    PluginLogger.LogWarning("CollectableItemManager instance is null in HeroController.Start postfix");
                    return;
                }

                Quest? quest = null;

                if (__instance is Quest q)
                {
                    quest = q;
                }
                else
                {
                    PluginLogger.LogWarning("FullQuestBase_BeginQuest_Patch. instance is not Quest");
                    return;
                }

                if (!(quest.QuestType.name == "Gather" || quest.QuestType.name == "Hunt"))
                {
                    PluginLogger.LogWarning("FullQuestBase_BeginQuest_Patch. quest is not Gather or Hunt");

                    return;
                }

                PluginLogger.LogInfo($"FullQuestBase_BeginQuest_Patch. Quest type:{quest.QuestType.name}");
                PluginLogger.LogInfo($"FullQuestBase_BeginQuest_Patch. Quest name:{quest.name}");

                IReadOnlyList<FullQuestBase.QuestTarget> targets = __instance.Targets;

                PluginLogger.LogInfo($"FullQuestBase_BeginQuest_Patch. Attempting iteration through targets");

                if (targets.Count == 0)
                {
                    PluginLogger.LogWarning($"FullQuestBase_BeginQuest_Patch. targets empty");
                }

                foreach (FullQuestBase.QuestTarget target in targets)
                    {
                        GatherOrHuntQuest.Data? d = GatherOrHuntQuest.FindByQuestNameAndTargetAmount(quest.name,
                        target.Count);

                        if (d == null)
                        {
                            PluginLogger.LogWarning($"FullQuestBase_BeginQuest_Patch. Not found data");

                            continue;
                        }

                        PluginLogger.LogInfo($"FullQuestBase_BeginQuest_Patch. Found Data.\n" +
                            $"QuestInternalName:    {d.QuestInternalName}\n" +
                            $"QuestExternalName:    {d.QuestExternalName}\n" +
                            $"QuestType:            {d.QuestType}\n" +
                            $"CollectableName:      {d.CollectableName}\n" +
                            $"TargetAmount:         {d.TargetAmount}");

                        QuestTargetCounter? questTargetCounter = target.Counter;

                        if (questTargetCounter == null)
                        {
                            PluginLogger.LogWarning($"FullQuestBase_BeginQuest_Patch. questTargetCounter is null");

                            continue;
                        }

                        CollectableItem? item = null;

                        if (questTargetCounter is CollectableItem item_)
                        {
                            item = item_;
                        }
                        else
                        {
                            PluginLogger.LogWarning($"FullQuestBase_BeginQuest_Patch. questTargetCounter is not CollectableItem");
                            continue;
                        }

                        //START DELAYED BY 5 SEC ASYNC Add(item, d.TargetAmount); Preferrably Coroutine
                        CoroutineRunner.Instance.RunCoroutine(DelayedAdd(item, d.TargetAmount));
                    }
            }
            else
            {
                PluginLogger.LogWarning("FullQuestBase_BeginQuest_Patch mode is NOT AutoComplete.Skip patch");
            }
        }

        private static IEnumerator DelayedAdd(CollectableItem item, int amount)
        {
            PluginLogger.LogInfo($"[Coroutine] Waiting 5 seconds before adding {item.name} x{amount}...");
            // yield return new WaitForSeconds(5f);
            yield return null;

            Add(item, amount);
        }
    }
    public class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner _instance;

        public static CoroutineRunner Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("EasyQuestsCoroutineRunner");
                    _instance = go.AddComponent<CoroutineRunner>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        public void RunCoroutine(IEnumerator routine)
        {
            StartCoroutine(routine);
        }
    }
}
