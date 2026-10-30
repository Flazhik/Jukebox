using System.IO;
using JukeboxCore.Utils;
using static JukeboxCore.Utils.CompositeSongsUtils;

namespace JukeboxCore.Subtitles;

public static class SubtitleFiles
{
    private const string Extension = ".srt";
    private const string IntroSuffix = "_intro";

    public static FileInfo MainSidecar(FileInfo track) => Sidecar(track, string.Empty);

    public static FileInfo IntroSidecar(FileInfo track) => Sidecar(track, IntroSuffix);

    private static FileInfo Sidecar(FileInfo track, string suffix)
    {
        if (track == null)
            return null;

        var baseName = GetBaseName(PathsUtils.GetFileNameWithoutExtension(track.Name));
        return new FileInfo(Path.Combine(track.DirectoryName ?? string.Empty, $"{baseName}{suffix}{Extension}"));
    }
}
