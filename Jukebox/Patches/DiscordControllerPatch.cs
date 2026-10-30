using System;
using Discord;
using HarmonyLib;
using Jukebox.Components;
using static JukeboxCore.Utils.ReflectionUtils;

namespace Jukebox.Patches;

[HarmonyPatch(typeof(DiscordController))]
public static class DiscordControllerPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(DiscordController), "UpdateWave")]
    public static bool DiscordController_UpdateWave_Prefix(int wave)
    {
        var player = JukeboxManager.Instance?.player;
        if (player == null || player.State.Song == null ||
            !PrefsManager.Instance.GetBoolLocal("jukebox.discordAndSteamIntegration"))
            return true;

        if (DiscordController.Instance == null)
            return false;

        var cachedActivity = GetPrivate<Activity>(DiscordController.Instance, typeof(DiscordController), "cachedActivity");
        var detailsBackup = cachedActivity.Details;

        try
        {
            var disabled = GetPrivate<bool>(DiscordController.Instance, typeof(DiscordController), "disabled");
            var activityManager = GetPrivate<ActivityManager>(DiscordController.Instance, typeof(DiscordController),
                "activityManager");
            var discord = GetPrivate<Discord.Discord>(DiscordController.Instance, typeof(DiscordController), "discord");

            if (disabled)
                return false;

            cachedActivity.Details = $"WAVE: {wave} | 🎵 {player.State.Song.ArtistAndTrack}";
            SetPrivate(DiscordController.Instance, typeof(DiscordController), "cachedActivity", cachedActivity);

            if (discord == null || activityManager == null)
                return false;

            activityManager.UpdateActivity(cachedActivity, static _ => { });
            return false;
        }
        catch (Exception)
        {
            cachedActivity.Details = detailsBackup;
            SetPrivate(DiscordController.Instance, typeof(DiscordController), "cachedActivity", cachedActivity);
            return true;
        }
    }
}