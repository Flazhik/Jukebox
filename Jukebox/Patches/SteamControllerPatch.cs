using HarmonyLib;
using Jukebox.Components;
using Steamworks;

namespace Jukebox.Patches
{
    [HarmonyPatch(typeof(SteamController))]
    public class SteamControllerPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SteamController), "UpdateWave")]
        public static bool SteamController_UpdateWave_Prefix(int wave)
        {
            if (JukeboxMusicPlayer.CurrentSong == null || !PrefsManager.Instance.GetBoolLocal("jukebox.discordAndSteamIntegration"))
                return true;
            
            if (!SteamClient.IsValid)
                return false;
            
            SteamFriends.SetRichPresence(nameof (wave), $"WAVE: {wave} | 🎵 {JukeboxMusicPlayer.CurrentSong.ArtistAndTrack}");
            return false;
        }
    }
}