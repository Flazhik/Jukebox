using BepInEx;

namespace JukeboxDownloader
{
    [BepInProcess("ULTRAKILL.exe")]
    [BepInDependency("dev.flazhik.jukebox")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]
    public class JukeboxDownloaderPlugin : BaseUnityPlugin
    {
    }
}