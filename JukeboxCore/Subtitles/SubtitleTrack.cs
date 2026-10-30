using System.Collections.Generic;

namespace JukeboxCore.Subtitles;

public sealed class SubtitleTrack(List<SubtitleLine> lines)
{
    public static readonly SubtitleTrack Empty = new([]);

    public IReadOnlyList<SubtitleLine> Lines => lines;
    private bool IsEmpty => lines.Count == 0;

    public int IndexAt(float time, int previous)
    {
        if (IsEmpty)
            return -1;

        var i = previous;
        if (i >= lines.Count)
            i = lines.Count - 1;
        
        if (i < 0)
            i = 0;

        while (i > 0 && lines[i].Start > time)
            i--;
        while (i + 1 < lines.Count && lines[i + 1].Start <= time)
            i++;

        return time >= lines[i].Start && time <= lines[i].End ? i : -1;
    }
}
