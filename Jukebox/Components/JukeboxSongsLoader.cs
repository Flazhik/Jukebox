using System.Collections.Generic;
using Jukebox.Core.Model.Song;

namespace Jukebox.Components
{
    [ConfigureSingleton(SingletonFlags.NoAutoInstance | SingletonFlags.DestroyDuplicates)]
    public class JukeboxSongsLoader: MonoSingleton<JukeboxSongsLoader>
    {
        private readonly Dictionary<Playlist.SongIdentifier, JukeboxSong> cache = new();

        public JukeboxSong Load(Playlist.SongIdentifier id)
        {
            if (cache.TryGetValue(id, out var result))
                return result;

            cache.Add(id, JukeboxSongFactory.Create(id));
            return cache[id];
        }
    }
}