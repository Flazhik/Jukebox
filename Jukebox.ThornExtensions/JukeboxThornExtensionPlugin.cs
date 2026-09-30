using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using JukeboxCore.Assets;

namespace Jukebox.ThornExtensions;

[BepInProcess("ULTRAKILL.exe")]
[BepInDependency(ThornPluginInfo.Guid, BepInDependency.DependencyFlags.SoftDependency)]
[BepInPlugin(PluginInfo.Guid, PluginInfo.Name, PluginInfo.Version)]
public class JukeboxThornExtensionPlugin : BaseUnityPlugin
{
    private static Harmony _harmony;
    
    private void Awake()
    {
        if (!Chainloader.PluginInfos.ContainsKey(ThornPluginInfo.Guid))
            return;
        
        AssetsManager.Inject(typeof(JukeboxThornExtensionPlugin).Assembly);
        _harmony = new Harmony(PluginInfo.Guid);
        _harmony.PatchAll();
    }
}