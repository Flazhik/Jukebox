using HarmonyLib;
using Jukebox.Components;

namespace Jukebox.Patches
{
    [HarmonyPatch(typeof(ScreenZone))]
    public class ScreenZonePatch
    {
        private static PreviewHelper helper;
        private static ScreenZone cgTerminal;
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ScreenZone), "Awake")]
        public static void ScreenZone_Awake_Postfix(ScreenZone __instance)
        {
            if (__instance.name != "Cybergrind Shop")
                return;

            if (!__instance.TryGetComponent<PreviewHelper>(out _)) 
                __instance.gameObject.AddComponent<PreviewHelper>();

            cgTerminal = __instance;
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ScreenZone), "Update")]
        public static bool ScreenZone_Update_Prefix(bool ___inZone, ScreenZone __instance)
        {
            if (cgTerminal != __instance)
                return true;

            if (helper == null)
            {
                if (!__instance.TryGetComponent<PreviewHelper>(out var h))
                    return true;
                helper = h;
            }

            helper.inZone = ___inZone;
            return true;
        }
    }
}