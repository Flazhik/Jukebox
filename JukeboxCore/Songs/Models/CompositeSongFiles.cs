using System.IO;

namespace JukeboxCore.Songs.Models;

public sealed class CompositeSongFiles
{
    public static readonly CompositeSongFiles Empty = new();

    public FileInfo Main { get; init; }
    public FileInfo Intro { get; init; }
    public FileInfo CalmIntro { get; init; }
    public FileInfo Calm { get; init; }

    public bool IsPlayable => Main != null;
    public bool HasIntro => Intro != null;
}
