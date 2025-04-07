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
            if (JukeboxManager.Instance.player != null)
                JukeboxManager.Instance.player.Stop();

            return true;
        }
    }
}