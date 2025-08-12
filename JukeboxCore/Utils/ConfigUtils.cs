using BepInEx.Configuration;
using static JukeboxCore.Utils.PathsUtils;

namespace JukeboxCore.Utils
{
    public static class ConfigUtils
    {
        public static readonly ConfigFile BepInExConfig = new(BepInExConfigPath, true);
    }
}