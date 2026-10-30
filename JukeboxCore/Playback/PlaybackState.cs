using JukeboxCore.Songs.Models;
using JukeboxCore.Subtitles;

namespace JukeboxCore.Playback;

public sealed class PlaybackState
{
    public JukeboxSong Song { get; set; }
    public int SongIndex { get; set; }
    public int ClipIndex { get; set; } = -1;
    public int ClipCount { get; set; }
    public float VolumeBoost { get; set; }
    public bool Stopped { get; set; }
    public SubtitleTrack Subtitles { get; set; }
    public string SubtitleLine { get; set; }
}
