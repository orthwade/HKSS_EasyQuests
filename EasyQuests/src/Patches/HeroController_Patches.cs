using HarmonyLib;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace owd.EasyQuests.Patches
{
    public static class CollectableItemDebug
    {
        public static string GetItemInfo(CollectableItem item)
        {
            if (item == null)
                return "[Null CollectableItem]";

            var sb = new StringBuilder();
            sb.AppendLine($"=== {item.GetDisplayName(CollectableItem.ReadSource.Inventory)} ===");
            // sb.AppendLine($"Description: {item.GetDescription(CollectableItem.ReadSource.Inventory)}");
            // sb.AppendLine($"Collected: {item.CollectedAmount}");
            // sb.AppendLine($"At Max: {item.IsAtMax()}");
            // sb.AppendLine($"Visible: {item.IsVisible}");
            // sb.AppendLine($"Consumable: {item.IsConsumable()}");
            // sb.AppendLine($"Can Consume Now: {item.CanConsumeRightNow()}");
            var quest = item.UseQuestForCap;
            if (quest != null)
            {
                sb.AppendLine($"Quest: {item.UseQuestForCap.name}");
            }


            sb.AppendLine();

            // Get UseResponses via reflection
            var getUseResponsesMethod = typeof(CollectableItem).GetMethod(
                "GetUseResponses",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            if (getUseResponsesMethod != null)
            {
                var responses = getUseResponsesMethod.Invoke(item, null) as IEnumerable<object>;

                if (responses != null && responses.Any())
                {
                    sb.AppendLine("Use Responses:");
                    foreach (var r in responses)
                    {
                        var t = r.GetType();
                        var useType = t.GetField("UseType")?.GetValue(r);
                        var getAmount = t.GetMethod("GetAmount");
                        var getAmountText = t.GetMethod("GetAmountText");
                        var descField = t.GetField("Description");

                        var amount = getAmount?.Invoke(r, null);
                        var amountText = getAmountText?.Invoke(r, null);
                        var desc = descField?.GetValue(r)?.ToString();

                        sb.AppendLine($"  - {useType} | Amount: {amountText ?? amount} | Desc: {desc}");
                    }
                }
                else
                {
                    // sb.AppendLine("Use Responses: None");
                }
            }
            else
            {
                sb.AppendLine("Use Responses: [Unable to access]");
            }

            return sb.ToString();
        }
    }

    [HarmonyPatch(typeof(HeroController))]
    [HarmonyPatch("Start")]
    internal static class HeroController_Start
    {
        private static void Postfix(HeroController __instance)
        {
            if (__instance == null)
            {
                // PluginLogger.LogWarning("HeroController instance is null in Start postfix");
                return;
            }
            CheckAndApplyAutoComplete.Do();
        }
    }
}
