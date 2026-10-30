using System;
using JukeboxCore.Songs.Models;
using static Playlist.SongIdentifier.IdentifierType;

namespace JukeboxCore.Songs;

public static class JukeboxSongSessionFactory
{
    public static JukeboxSongSession Create(JukeboxSong song)
    {
        return song.Id.type switch
        {
            File => CustomSoundtrackSession(song),
            Addressable => new JukeboxSoundtrackSongSession(song),
            _ => throw new ArgumentOutOfRangeException(nameof(song))
        };
    }

    private static JukeboxSongSession CustomSoundtrackSession(JukeboxSong song)
    {
        return song.Metadata is JukeboxCustomSongMetadata { Files.IsPlayable: true } custom
            ? new JukeboxCustomSongSession(song, custom.Files)
            : null;
    }
}
