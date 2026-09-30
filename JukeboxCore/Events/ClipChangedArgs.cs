using JukeboxCore.Songs.Models;

namespace JukeboxCore.Events;

public class ClipChangedArgs(JukeboxSong song, bool newSong)
{
    public JukeboxSong Song { get; private set; } = song;
    public bool NewSong { get; private set; } = newSong;
}