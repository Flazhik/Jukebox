using System.IO;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;
using YtUtils = YoutubeDLSharp.Utils;

namespace JukeboxDownloader.Service.YouTube;

internal sealed class JukeboxYoutubeDL(byte maxNumberOfProcesses = 4) : YoutubeDL(maxNumberOfProcesses)
{
    protected override OptionSet GetDownloadOptions()
    {
        return new OptionSet
        {
            IgnoreErrors = IgnoreDownloadErrors,
            IgnoreConfig = true,
            Downloader = "m3u8:native",
            DownloaderArgs = "ffmpeg:-nostats -loglevel 0",
            Output = Path.Combine(OutputFolder, OutputFileTemplate),
            RestrictFilenames = RestrictFilenames,
            ForceOverwrites = OverwriteFiles,
            NoOverwrites = !OverwriteFiles,
            NoPart = true,
            FfmpegLocation = YtUtils.GetFullPath(FFmpegPath),
            Progress = true,
            Print = "after_move:outfile: %(filepath)s"
        };
    }
}