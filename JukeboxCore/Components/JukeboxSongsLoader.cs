using System.Collections.Generic;
using JukeboxCore.Songs;
using JukeboxCore.Songs.Models;

namespace JukeboxCore.Components;

[ConfigureSingleton(SingletonFlags.NoAutoInstance | SingletonFlags.DestroyDuplicates)]
public class JukeboxSongsLoader : MonoSingleton<JukeboxSongsLoader>
{
    private readonly Dictionary<Playlist.SongIdentifier, JukeboxSong> cache = new();

    public JukeboxSong Get(Playlist.SongIdentifier id)
    {
        if (cache.TryGetValue(id, out var song))
            return song;

        song = JukeboxSong.Create(id);
        cache.Add(id, song);
        return song;
    }

    public JukeboxSongSession CreateSession(JukeboxSong song) => song != null
        ? JukeboxSongSessionFactory.Create(song)
        : null;
}
