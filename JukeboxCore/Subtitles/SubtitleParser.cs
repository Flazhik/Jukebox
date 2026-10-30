using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JukeboxCore.Logging;
using SubtitlesParser.Classes;
using SubtitlesParser.Classes.Parsers;

namespace JukeboxCore.Subtitles;

public static class SubtitleParser
{
    public static SubtitleTrack FromFile(FileInfo file)
    {
        if (file is not { Exists: true })
            return SubtitleTrack.Empty;
        try
        {
            using var stream = File.OpenRead(file.FullName);
            return ToSubtitlesTrack(new SrtParser().ParseStream(stream, Encoding.UTF8));
        }
        catch (Exception e)
        {
            JukeboxLogger.Log.LogError($"Couldn't read subtitles from {file.FullName}\n{e}");
            return SubtitleTrack.Empty;
        }
    }

    private static SubtitleTrack ToSubtitlesTrack(List<SubtitleItem> items)
    {
        var lines = new List<SubtitleLine>();
        foreach (var item in items)
        {
            var text = string.Join("\n", item.Lines);
            if (text.Length == 0)
                continue;

            lines.Add(new SubtitleLine(item.StartTime / 1000f, item.EndTime / 1000f, text));
        }

        return lines.Count != 0
            ? new SubtitleTrack(lines)
            : SubtitleTrack.Empty;
    }
}
