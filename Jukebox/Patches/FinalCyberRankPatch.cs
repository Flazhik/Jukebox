using HarmonyLib;
using Jukebox.Components;

namespace Jukebox.Patches
{
    [HarmonyPatch(typeof(FinalCyberRank))]
    public static class FinalCyberRankPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FinalCyberRank), "GameOver")]
        public static bool FinalCyberRank_GameOver_Prefix()
        {
            var player = JukeboxManager.Instance.player;
            if (player != null)
            {
                if (PrefsManager.Instance.GetBoolLocal("jukebox.resumeLastSong")
                    && JukeboxMusicPlayer.CurrentSong != null
                    && player.Source.clip != null)
                {
                    var preferences = JukeboxPreferencesManager.Instance;
                    preferences.SetPlaybackPosition(JukeboxMusicPlayer.CurrentSong.Id, JukeboxMusicPlayer.CurrentClipIndex, player.Source.time);
                    preferences.ForceSavePreferences();
                }
                player.Stop();
            }

            return true;
        }
    }
}