using BepInEx;

namespace JukeboxDownloader
{
    [BepInProcess("ULTRAKILL.exe")]
    [BepInDependency("dev.flazhik.jukebox")]
    [BepInPlugin(PluginInfo.Guid, PluginInfo.Name, PluginInfo.Version)]
    public class JukeboxDownloaderPlugin : BaseUnityPlugin
    {
    }
}