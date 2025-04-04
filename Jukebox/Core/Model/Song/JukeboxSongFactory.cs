using System;
using static Playlist.SongIdentifier.IdentifierType;
using SongIdentifier = Playlist.SongIdentifier;

namespace Jukebox.Core.Model.Song    
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