using HarmonyLib;
using Jukebox.Components;

namespace Jukebox.Patches;

[HarmonyPatch(typeof(FinalCyberRank))]
public static class FinalCyberRankPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(FinalCyberRank), "GameOver")]
    public static void FinalCyberRank_GameOver_Prefix()
    {
        var player = JukeboxManager.Instance?.player;
        if (player != null)
        {
            if (PrefsManager.Instance.GetBoolLocal("jukebox.resumeLastSong")
                && player.State.Song != null
                && player.Source.clip != null)
            {
                var preferences = JukeboxPreferencesManager.Instance;
                preferences.SetPlaybackPosition(player.State.Song.Id, player.State.ClipIndex, player.Source.time);
                preferences.ForceSavePreferences();
            }
        }
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(FinalCyberRank), "Appear")]
    public static void FinalCyberRank_Appear_Postfix(FinalCyberRank __instance)
    {
        if (JukeboxManager.Instance?.player is {} player && __instance.toAppear[0].activeSelf && !player.State.Stopped)
            JukeboxManager.Instance?.player.Stop();
    }
}