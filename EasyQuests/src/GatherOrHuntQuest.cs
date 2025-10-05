using System.Collections.Generic;
using System.Linq;

namespace owd.EasyQuests
{
    /// <summary>
    /// Provides static lookup and retrieval utilities for Hunt and Gather quests.
    /// </summary>
    public static class GatherOrHuntQuest
    {
        public static void PrintGatherAndHutQuestsNames()
        {
            var list_ = QuestManager.GetAllQuests();

            PluginLogger.LogInfo($"FullQuestBase list");

            foreach (BasicQuestBase bqb in list_)
            {
                if (bqb is FullQuestBase fq)
                {

                    if (fq.QuestType.name == "Gather" || fq.QuestType.name == "Hunt")
                    {
                        PluginLogger.LogInfo($"Found quest:\n" +
                        $"Name: {fq.name}\n" +
                        $"Type: {fq.QuestType.name}");
                    }
                }
            }
            
            PluginLogger.LogInfo($"\n\n\n");
            PluginLogger.LogInfo($"Quest list");

            foreach (BasicQuestBase bqb in list_)
            {
                if (bqb is Quest q)
                {
                    if (q.QuestType.name == "Gather" || q.QuestType.name == "Hunt")
                    {
                        string str = "Found quest:\n";
                        str += $"Name: {q.name}\n";
                        str += $"Type: {q.QuestType.name}";
                        str += "Target Amount:\n";

                        foreach (FullQuestBase.QuestTarget t in q.Targets)
                        {
                            str += $"{t.Count}\n";
                        }

                        PluginLogger.LogInfo(str);
                    }
                }
            }
        }
        /// <summary>
        /// Represents a single quest definition.
        /// </summary>
        public class Data
        {
            public readonly string QuestInternalName;
            public readonly string QuestExternalName;
            public readonly string QuestType;
            public readonly string CollectableName;
            public readonly int TargetAmount;

            public Data(string questInternalName, string questExternalName, string questType, string collectableName, int targetAmount)
            {
                QuestInternalName = questInternalName;
                QuestExternalName = questExternalName;
                QuestType = questType;
                CollectableName = collectableName;
                TargetAmount = targetAmount;
            }
        }

        // Base list of all quests
        private static readonly List<Data> allQuests = new List<Data>()
        {
            //TODO 
            new Data("Shell Flowers"         , "Rite of the Pollip"     , "Gather"  , "Shell Flower", 6),
            new Data("Shiny Bell Goomba"     , "Silver Bells"           , "Gather"  , "Silver Bellclapper", 8),
            new Data("Extractor Blue"        , "Alchemist's Assistant"  , "Gather"  , "Plasmium", 3),
            new Data("Extractor Blue Worms"  , "Advanced Alchemy"       , "Gather"  , "Plasmium Blood", 10),

            new Data("Huntress Quest"       , "Broodfeast"            , "Hunt"  , "Enemy Morsel Seared", 15),
            new Data("Huntress Quest"       , "Broodfeast"            , "Hunt"  , "Enemy Morsel Shredded", 35),
            new Data("Huntress Quest"       , "Broodfeast"            , "Hunt"  , "Enemy Morsel Speared", 10),
            new Data("Pilgrim Rags"         , "Garb of the Pilgrims"  , "Hunt"  , "Pilgrim Rag", 12),
            new Data("Brolly Get"           , "Flexile Spines"        , "Hunt"  , "Common Spine", 25),
            new Data("Fine Pins"            , "Fine Pins"             , "Hunt"  , "Fine Pin", 10),
            new Data("Song Pilgrim Cloaks"  , "Cloaks of the Choir"   , "Hunt"  , "Song Pilgrim Cloak", 15),
            new Data("Crow Feathers"        , "Crawbug Clearing"      , "Hunt"  , "Crow Feather", 25),
            new Data("Rock Rollers"         , "Volatile Flintbeetles" , "Hunt"  , "Rock Roller Item", 3),
            new Data("Roach Killing"        , "Roach Guts"            , "Hunt"  , "Roach Corpse Item", 10)
        };

        /// <summary>
        /// Lookup table keyed by "QuestInternalName:TargetAmount".
        /// </summary>
        private static readonly Dictionary<string, Data> questsByNameAndTarget =
            allQuests.ToDictionary(
                q => $"{q.QuestInternalName}:{q.TargetAmount}",
                q => q
            );

        /// <summary>
        /// Lookup table keyed by CollectableName.
        /// </summary>
        private static readonly Dictionary<string, Data> questsByCollectable =
            allQuests.ToDictionary(
                q => q.CollectableName,
                q => q
            );

        /// <summary>
        /// Provides a read-only view of all defined quests.
        /// </summary>
        public static IReadOnlyList<Data> AllQuests => allQuests;

        /// <summary>
        /// Find a quest by its internal name and target amount.
        /// </summary>
        public static Data? FindByQuestNameAndTargetAmount(string questName, int targetAmount)
        {
            string key = $"{questName}:{targetAmount}";
            questsByNameAndTarget.TryGetValue(key, out var quest);
            return quest;
        }

        /// <summary>
        /// Find a quest by its collectable name.
        /// </summary>
        public static Data? FindByCollectableName(string collectableName)
        {
            questsByCollectable.TryGetValue(collectableName, out var quest);
            return quest;
        }
    }
}
