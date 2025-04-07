using HarmonyLib;
using Jukebox.Components;

namespace Jukebox.Patches
{
    [HarmonyPatch(typeof(AudioMixerController))]
    public static class AudioMixerControllerPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(AudioMixerController), "Update")]
        public static void AudioMixerController_Update_Postfix(AudioMixerController __instance)
        {
            if (JukeboxManager.Instance == null)
                return;
            
            __instance.musicSound.SetFloat("allVolume",
                __instance.CalculateVolume(__instance.musicVolume) + JukeboxManager.Instance.player.VolumeBoost);
        }
    }
}