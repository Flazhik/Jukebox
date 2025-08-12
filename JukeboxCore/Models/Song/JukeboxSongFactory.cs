using System;
using SongIdentifier = Playlist.SongIdentifier;
using static Playlist.SongIdentifier.IdentifierType;

namespace JukeboxCore.Models.Song    
{
    public static class JukeboxSongFactory
    {
        public static JukeboxSong Create(SongIdentifier id)
        {
            return id.type switch
            {
                File => JukeboxCustomSong.Create(id.path),
                Addressable => JukeboxSoundtrackSong.Create(id.path),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}