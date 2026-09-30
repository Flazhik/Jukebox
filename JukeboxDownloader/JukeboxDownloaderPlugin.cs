using BepInEx;
using JukeboxCore.Logging;

namespace JukeboxDownloader;

[BepInProcess("ULTRAKILL.exe")]
[BepInDependency("dev.flazhik.jukebox")]
[BepInPlugin(PluginInfo.Guid, PluginInfo.Name, PluginInfo.Version)]
public class JukeboxDownloaderPlugin : BaseUnityPlugin
{
    private void Awake()
    {
        JukeboxLogger.Initialize(Logger);
    }

    private void OnDestroy()
    {
        JukeboxLogger.Shutdown();
    }
}
