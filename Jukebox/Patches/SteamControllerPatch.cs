using HarmonyLib;
using Jukebox.Components;
using Steamworks;

namespace Jukebox.Patches;

[HarmonyPatch(typeof(SteamController))]
public static class SteamControllerPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(SteamController), "UpdateWave")]
    public static bool SteamController_UpdateWave_Prefix(int wave)
    {
        var player = JukeboxManager.Instance?.player;
        if (player == null || player.State.Song == null ||
            !PrefsManager.Instance.GetBoolLocal("jukebox.discordAndSteamIntegration"))
            return true;

        if (!SteamClient.IsValid)
            return false;

        SteamFriends.SetRichPresence(nameof(wave), $"WAVE: {wave} | 🎵 {player.State.Song.ArtistAndTrack}");
        return false;
    }
}