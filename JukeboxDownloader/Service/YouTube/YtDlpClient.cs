using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BepInEx.Configuration;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;
using static JukeboxCore.Utils.PathsUtils;
using static JukeboxCore.Utils.ConfigUtils;
using static JukeboxCore.Logging.JukeboxLogger;

namespace JukeboxDownloader.Service.YouTube;

public sealed class YtDlpClient
{
    private const string PostProcessing = "EmbedThumbnail+ffmpeg_o:-c:v mjpeg -vf crop=\\\"w='min(iw,ih)':h='min(iw,ih)'\\\"";
    private const string EnglishLanguage = "en";

    private static readonly string OutputFolder = Path.Combine(MusicPath, "YouTube");
    private static readonly Progress<string> Logger = new(static msg => Log.LogInfo(msg));

    private readonly JukeboxYoutubeDL client;
    
    private readonly string cookiesPath = Path.Combine(AssemblyPath, "cookies.txt");
    private readonly ConfigEntry<byte> ytDlpMaxProcesses = BepInExConfig.Bind("Concurrency",
        "YtDlpMaxProcesses",
        (byte)3,
        "Maximum number of yt-dlp processes");

    public YtDlpClient()
    {
        client = new JukeboxYoutubeDL(ytDlpMaxProcesses.Value)
        {
            YoutubeDLPath = Path.Combine(AssemblyPath, "yt-dlp.exe"),
            FFmpegPath = "ffmpeg.exe"
        };
    }

    private static bool SaveIntoSeparateFolders => PrefsManager.Instance!.GetBoolLocal("jukebox.downloader.separateFolderPerPlaylist");

    public async Task<RunResult<VideoData>> GetMetadata(string url, CancellationToken token)
    {
        var options = new OptionSet
        {
            Cookies = cookiesPath
        };

        return await client.RunVideoDataFetch(url, overrideOptions: options, ct: token, fetchComments: false);
    }

    public async Task<RunResult<string>> Download(
        string url,
        string playlist,
        IProgress<DownloadProgress> progress,
        CancellationToken token)
    {
        var outputFolder = !string.IsNullOrEmpty(playlist) || !SaveIntoSeparateFolders
            ? Path.Combine(OutputFolder, CoerceValidFileName(playlist))
            : OutputFolder;

        var options = new OptionSet
        {
            ExtractAudio = true,
            EmbedThumbnail = true,
            EmbedMetadata = true,
            AudioFormat = AudioConversionFormat.Mp3,
            WriteSubs = true,
            SubLangs = $"{EnglishLanguage}.*",
            Output = $"{outputFolder}\\%(title)s.%(ext)s",
            Cookies = cookiesPath
        };

        options.AddCustomOption("--ppa", PostProcessing);
        options.AddCustomOption("--convert-subs", "srt");

        var result = await client.RunWithOptions(url, options, progress: progress, ct: token, output: Logger, showArgs: true);
        if (result.Success)
            MoveSubtitles(outputFolder);

        return result;
    }

    private static void MoveSubtitles(string folder)
    {
        foreach (var source in Directory.GetFiles(folder, "*.srt"))
        {
            var name = Path.GetFileNameWithoutExtension(source);
            var separator = name.LastIndexOf('.');
            if (separator < 0)
                continue;

            if (!name[(separator + 1)..].StartsWith(EnglishLanguage, StringComparison.OrdinalIgnoreCase))
                continue;

            var dst = Path.Combine(folder, $"{name[..separator]}.srt");

            if (File.Exists(dst))
                File.Delete(dst);

            File.Move(source, dst);
        }
    }
}
