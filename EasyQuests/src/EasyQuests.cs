using BepInEx;
using HarmonyLib;
using Mono.Cecil;
using System;
using System.IO;
using System.Reflection;

namespace owd.EasyQuests
{
    [BepInPlugin("com.orthwade.EasyQuests", "Easy Quests", "0.2.0")]
    public class EasyQuests : BaseUnityPlugin
    {
        internal static EasyQuests Instance;
        private void Awake()
        {
            Instance = this;

            PluginLogger.Init(Config);

            // Load the embedded quests.json directly into a string
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("EasyQuests.quests.json");
            using var reader = new StreamReader(stream);
            string json = reader.ReadToEnd();

            // Now feed it to your existing logic (just slightly adjusted)
            QuestDatabase.LoadFromJsonString(json);

            
            Conf.Init(Config);

            PluginLogger.LogInfo("Easy Quests loaded!");

            var harmony = new Harmony("com.orthwade.EasyQuests");
            harmony.PatchAll();


            // FSMScanner.Scan();
        }
    }
}
