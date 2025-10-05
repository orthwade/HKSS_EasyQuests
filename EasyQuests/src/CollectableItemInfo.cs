using System.Collections.Generic;

namespace owd.EasyQuests
{

    public static class CollectableItemManagerExtensions
    {
        // private static List<CollectableItemInfo> dataList = new List<CollectableItemInfo>();

        public static List<CollectableItemInfo> GetCollectableData(this CollectableItemManager manager)
        {
            PluginLogger.LogInfo("GetCollectableData");

            var dataList = new List<CollectableItemInfo>();

            foreach (CollectableItem item_ in manager.GetAllCollectables())
            {
                if (item_ == null)
                {
                    PluginLogger.LogWarning($" item is null");
                    continue;
                }

                Quest q = item_.UseQuestForCap;

                if (q == null)
                    continue;

                dataList.Add(new CollectableItemInfo()
                {
                    item = item_,
                    quest = q
                });
            }

            return dataList;
        }
    }

    public class CollectableItemInfo
    {
        public CollectableItem item;
        public Quest quest;
    }
}
