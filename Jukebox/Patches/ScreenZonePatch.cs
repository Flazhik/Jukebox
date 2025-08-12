using HarmonyLib;
using Jukebox.Components;

namespace Jukebox.Patches
{
    [HarmonyPatch(typeof(ScreenZone))]
    public static class ScreenZonePatch
    {
        private static PreviewHelper _helper;
        private static ScreenZone _cgTerminal;
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ScreenZone), "Awake")]
        public static void ScreenZone_Awake_Postfix(ScreenZone __instance)
        {
            if (__instance.name != "Cybergrind Shop")
                return;

            if (!__instance.TryGetComponent<PreviewHelper>(out _)) 
                __instance.gameObject.AddComponent<PreviewHelper>();

            _cgTerminal = __instance;
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ScreenZone), "Update")]
        public static bool ScreenZone_Update_Prefix(bool ___inZone, ScreenZone __instance)
        {
            if (_cgTerminal != __instance)
                return true;

            if (_helper == null)
            {
                if (!__instance.TryGetComponent<PreviewHelper>(out var h))
                    return true;
                _helper = h;
            }

            _helper.inZone = ___inZone;
            return true;
        }
    }
}