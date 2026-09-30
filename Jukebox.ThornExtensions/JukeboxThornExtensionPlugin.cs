using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using Jukebox.ThornExtensions.Modules;
using UnityEngine;

namespace Jukebox.ThornExtensions;

[BepInProcess("ULTRAKILL.exe")]
[BepInDependency(JukeboxPluginInfo.Guid)]
[BepInDependency(ThornPluginInfo.Guid, BepInDependency.DependencyFlags.SoftDependency)]
[BepInPlugin(PluginInfo.Guid, PluginInfo.Name, PluginInfo.Version)]
public class JukeboxThornExtensionPlugin : BaseUnityPlugin
{
    private static Harmony _harmony;

    private void Awake()
    {
        if (!ThornPresent)
            return;

        gameObject.hideFlags = HideFlags.HideAndDontSave;
        _harmony = new Harmony(PluginInfo.Guid);
        _harmony.PatchAll();
    }

    private void Start()
    {
        if (!ThornPresent)
            return;

        NowPlayingModule.Load();
        PlaybackTimeModule.Load();
        SubtitlesModule.Load();
    }

    private static bool ThornPresent => Chainloader.PluginInfos.ContainsKey(ThornPluginInfo.Guid);
}
