using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Reflection;

namespace owd.EasyQuests
{
    public enum QuestType
    {
        Hunt,
        Gather
    }

    public enum TargetType
    {
        Item,
        Enemy
    }

    public sealed class QuestTargetData
    {
        public string CounterName { get; set; }
        public int Amount { get; set; }
        public string CounterClassName { get; set; }
        public TargetType TargetType { get; set; }

        [JsonIgnore]
        public Type CounterClassType { get; set; }

        // Parameterless ctor for JSON deserialization
        public QuestTargetData()
        {
        }

        // Convenience ctor for code-based creation (if you ever need)
        public QuestTargetData(string counterName, int amount, string counterClassName, TargetType targetType = TargetType.Item)
        {
            CounterName = counterName;
            Amount = amount;
            CounterClassName = counterClassName;
            TargetType = targetType;
            CounterClassType = ResolveType(counterClassName);
        }

        private static Type ResolveType(string className)
        {
            if (string.IsNullOrWhiteSpace(className))
                return null;

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = asm.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types;
                }

                if (types == null)
                    continue;

                foreach (var t in types)
                {
                    if (t == null)
                        continue;

                    if (t.Name.Equals(className, StringComparison.OrdinalIgnoreCase))
                        return t;
                }
            }

            return null;
        }
    }

    public sealed class QuestData
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public QuestType Type { get; set; }
        public List<QuestTargetData> Targets { get; set; } = new List<QuestTargetData>();

        public QuestData()
        {
        }

        public QuestData(string name, string displayName, QuestType type, List<QuestTargetData> targets)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            Type = type;
            Targets = targets ?? new List<QuestTargetData>();
        }
    }

    public static class QuestDatabase
    {
        // Starts empty; call LoadFromJson to populate
        public static List<QuestData> Quests { get; private set; } = new List<QuestData>();

        public static IReadOnlyDictionary<string, List<QuestData>> QuestsByCounterName { get; private set; }
        public static IReadOnlyDictionary<string, List<QuestData>> QuestsByDisplayName { get; private set; }


        /// <summary>
        /// Load quests from a JSON file (Newtonsoft.Json).
        /// </summary>
        /// <param name="filePath">Path to quests.json</param>
        public static void LoadFromJson(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("filePath is required", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Quest JSON not found: {filePath}");

            string json = File.ReadAllText(filePath);

            var settings = new JsonSerializerSettings();
            // Ensure enums are read/written as strings
            settings.Converters.Add(new StringEnumConverter());

            var loaded = JsonConvert.DeserializeObject<List<QuestData>>(json, settings);
            Quests = loaded ?? new List<QuestData>();

            // Resolve runtime-only Type references and optionally infer TargetType
            foreach (var quest in Quests)
            {
                if (quest.Targets == null)
                    continue;

                foreach (var target in quest.Targets)
                {
                    if (!string.IsNullOrWhiteSpace(target.CounterClassName))
                    {
                        target.CounterClassType = ResolveType(target.CounterClassName);
                    }

                    // Optional: basic inference if JSON didn't specify TargetType
                    // (if CounterClassName suggests enemy/tool-damage/journal)
                    if (!Enum.IsDefined(typeof(TargetType), target.TargetType))
                    {
                        target.TargetType = TargetType.Item;
                    }

                    if (!Enum.IsDefined(typeof(TargetType), target.TargetType))
                    {
                        target.TargetType = TargetType.Item;
                    }
                }
            }
            BuildDictionaries();
            PrintAllQuestsByCounterName();
            PrintAllQuestsByDisplayName();
        }

        private static void BuildDictionaries()
        {
            QuestsByCounterName = BuildCounterDictionary();
            QuestsByDisplayName = BuildDisplayNameDictionary();
        }

        private static IReadOnlyDictionary<string, List<QuestData>> BuildCounterDictionary()
        {
            var dict = new Dictionary<string, List<QuestData>>(StringComparer.OrdinalIgnoreCase);

            foreach (var quest in Quests)
            {
                foreach (var target in quest.Targets)
                {
                    if (string.IsNullOrWhiteSpace(target.CounterName))
                        continue;

                    if (!dict.TryGetValue(target.CounterName, out var list))
                    {
                        list = new List<QuestData>();
                        dict[target.CounterName] = list;
                    }

                    list.Add(quest);
                }
            }

            return dict;
        }

        private static IReadOnlyDictionary<string, List<QuestData>> BuildDisplayNameDictionary()
        {
            var dict = new Dictionary<string, List<QuestData>>(StringComparer.OrdinalIgnoreCase);

            foreach (var quest in Quests)
            {
                if (string.IsNullOrWhiteSpace(quest.DisplayName))
                    continue;

                if (!dict.TryGetValue(quest.DisplayName, out var list))
                {
                    list = new List<QuestData>();
                    dict[quest.DisplayName] = list;
                }

                list.Add(quest);
            }

            return dict;
        }

        // Optional helpers for debugging/logging
        public static void PrintAllQuestsByCounterName()
        {
            PluginLogger.LogInfo("=== Quests By Counter Name ===");
            foreach (var pair in QuestsByCounterName)
            {
                PluginLogger.LogInfo($"Counter: {pair.Key}");
                foreach (var qd in pair.Value)
                    PluginLogger.LogInfo($"  - {qd.DisplayName} ({qd.Name}) [{qd.Type}]");
            }
            PluginLogger.LogInfo("==============================");
        }

        public static void PrintAllQuestsByDisplayName()
        {
            PluginLogger.LogInfo("=== Quests By Display Name ===");
            foreach (var pair in QuestsByDisplayName)
            {
                PluginLogger.LogInfo($"Display Name: {pair.Key}");
                foreach (var qd in pair.Value)
                    PluginLogger.LogInfo($"  - {qd.Name} [{qd.Type}]");
            }
            PluginLogger.LogInfo("==============================");
        }

        /// <summary>
        /// Serialize the currently loaded quests back to JSON (useful for editors).
        /// </summary>
        public static string ToJson(bool indented = true)
        {
            var formatting = indented ? Formatting.Indented : Formatting.None;
            // Use overload that takes formatting + converters
            return JsonConvert.SerializeObject(Quests, formatting, new JsonConverter[] { new StringEnumConverter() });
        }

        private static Type ResolveType(string className)
        {
            if (string.IsNullOrWhiteSpace(className))
                return null;

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = asm.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types;
                }

                if (types == null)
                    continue;

                foreach (var t in types)
                {
                    if (t == null)
                        continue;

                    if (t.Name.Equals(className, StringComparison.OrdinalIgnoreCase))
                        return t;
                }
            }
            return null;
        }

        /// <summary> /// Dictionary mapping CounterName → List of QuestData that include it. /// </summary> public static readonly IReadOnlyDictionary<string, List<QuestData>> QuestsByCounterName = BuildCounterDictionary(); private static IReadOnlyDictionary<string, List<QuestData>> BuildCounterDictionary() { var dict = new Dictionary<string, List<QuestData>>(StringComparer.OrdinalIgnoreCase); foreach (var quest in Quests) { foreach (var target in quest.Targets) { if (string.IsNullOrWhiteSpace(target.CounterName)) continue; if (!dict.TryGetValue(target.CounterName, out var list)) { list = new List<QuestData>(); dict[target.CounterName] = list; } list.Add(quest); } } return dict; }
        /// 
        
    }
}
