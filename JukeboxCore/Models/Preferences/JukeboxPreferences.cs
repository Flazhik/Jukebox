using System.Collections.Generic;
using Newtonsoft.Json;

namespace JukeboxCore.Models.Preferences
{
    [JsonObject(MemberSerialization.OptIn)]
    public class JukeboxPreferences
    {
        [JsonProperty("volumeBoost", ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public Dictionary<string, float> boostSettings = new();
        
        [JsonProperty("playStaringFrom")]
        public PlayStartingFrom playTrackStaringFrom;

        [JsonObject(MemberSerialization.OptIn)]
        public class PlayStartingFrom
        {
            [JsonProperty("path")]
            public string Path { get; private set; }
            [JsonProperty("type")]
            public Playlist.SongIdentifier.IdentifierType Type { get; private set; }
            [JsonProperty("clip")]
            public int Clip { get; private set; }
            [JsonProperty("startFrom")]
            public float StartFrom { get; private set; }
            
            [JsonConstructor]
            protected PlayStartingFrom()
            {
            }

            public PlayStartingFrom(Playlist.SongIdentifier id, int clipIndex, float startFrom)
            {
                Path = id.path;
                Type = id.type;
                Clip = clipIndex;
                StartFrom = startFrom;
            }
        }
    }
}