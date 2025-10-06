using HarmonyLib;
using System.Collections.Generic;

namespace owd.EasyQuests.Access
{
    public static class HeroController_Access
    {
        private static readonly AccessTools.FieldRef<HeroController, float> preventCastByDialogueEndTimerRef =
        AccessTools.FieldRefAccess<HeroController, float>("preventCastByDialogueEndTimer");
        
        public static float GetPreventCastByDialogueEndTimer()
        {
            HeroController hero = HeroController.instance;
            if (hero == null)
                return 0.0f;

            return preventCastByDialogueEndTimerRef(hero);
        }
    }
}