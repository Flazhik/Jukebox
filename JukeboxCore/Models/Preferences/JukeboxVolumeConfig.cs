using System.Collections.Generic;
using Newtonsoft.Json;

namespace JukeboxCore.Models.Preferences
{
    [JsonObject(MemberSerialization.OptIn)]
    public class JukeboxVolumeConfig
    {
        [JsonProperty("volumeBoost", ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public Dictionary<string, float> boostSettings = new();
    }
}