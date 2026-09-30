using HarmonyLib;
using Jukebox.ThornExtensions.Modules;
using ThornClient.Core;

namespace Jukebox.ThornExtensions.Patches;

/**
 * Thorn recursively changes the fonts of all TMP components in a HUD module, which breaks my custom fonts
 *
 * This patch counters that behavior by applying my own overlay material
 */
[HarmonyPatch(typeof(HudModule))]
public static class HudModulePatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(HudModule), "Reparent")]
    public static void HudModule_Reparent_Postfix(HudModule __instance)
    {
        switch (__instance)
        {
            case NowPlayingModule npModule:
            {
                if (npModule.hud == null)
                    return;

                npModule.hud.RestoreFontMaterials();
                break;
            }
            
            case PlaybackTimeModule playbackModule:
            {
                if (playbackModule.hud == null)
                    return;

                playbackModule.hud.RestoreFontMaterials();
                break;
            }
            
            case SubtitlesModule subtitlesModule:
            {
                subtitlesModule.hud.RestoreFontMaterial();
                break;
            }
        }
    }
}