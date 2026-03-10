using JukeboxCore.Models.Song;

namespace JukeboxCore.Events
{
    public class ClipChangedArgs
    {
        public JukeboxSong Song { get; private set; }
        public bool NewSong { get; private set; }

        public ClipChangedArgs(JukeboxSong song, bool newSong)
        {
            Song = song;
            NewSong = newSong;
        }
    }
}