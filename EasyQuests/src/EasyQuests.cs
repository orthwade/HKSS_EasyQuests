using BepInEx;
using HarmonyLib;

namespace owd.EasyQuests
{
    [BepInPlugin("com.orthwade.EasyQuests", "Easy Quests", "0.1.0")]
    public class EasyQuests : BaseUnityPlugin
    {
        internal static EasyQuests Instance;
        private void Awake()
        {
            Instance = this;

            PluginLogger.Init(Config);

            QuestDatabase.LoadFromJson("BepInEx/plugins/EasyQuests/quests.json");
            
            Conf.Init(Config);

            PluginLogger.LogInfo("Easy Quests loaded!");

            var harmony = new Harmony("com.orthwade.EasyQuests");
            harmony.PatchAll();


            // FSMScanner.Scan();
        }
    }
}
