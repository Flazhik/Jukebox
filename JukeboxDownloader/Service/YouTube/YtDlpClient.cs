using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BepInEx.Configuration;
using JukeboxCore;
using UnityEngine;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;
using static JukeboxCore.Utils.PathsUtils;
using static JukeboxCore.Utils.ConfigUtils;

namespace JukeboxDownloader.Service.YouTube
{
    public class YtDlpClient
    {
        private readonly ConfigEntry<byte> ytDlpMaxProcesses = BepInExConfig.Bind("Concurrency",
            "YtDlpMaxProcesses",
            (byte)3,
            "Maximum number of yt-dlp processes");

        private readonly string cookiesPath = Path.Combine(AssemblyPath, "cookies.txt");

        private const string PostProcessing =
            "EmbedThumbnail+ffmpeg_o:-c:v mjpeg -vf crop=\\\"w='min(iw,ih)':h='min(iw,ih)'\\\"";

        private static bool SaveIntoSeparateFolders =>
            PrefsManager.Instance.GetBoolLocal("jukebox.downloader.separateFolderPerPlaylist");

        private static readonly string OutputFolder = Path.Combine(MusicPath, "YouTube");

        private static readonly Progress<string> Logger =
            new(msg => MainThreadDispatcher.Instance.Enqueue(() => Debug.Log(msg)));

        private readonly YoutubeDL client;

        public YtDlpClient()
        {
            client = new YoutubeDL(ytDlpMaxProcesses.Value)
            {
                YoutubeDLPath = Path.Combine(AssemblyPath, "yt-dlp.exe"),
                FFmpegPath ="ffmpeg.exe"
            };
        }

        public async Task<RunResult<VideoData>> GetMetadata(string url, CancellationToken token)
        {
            var options = new OptionSet
            {
                Cookies = cookiesPath
            };
            return await client.RunVideoDataFetch(url, overrideOptions: options, ct: token);
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
                Output = $"{outputFolder}\\%(title)s.%(ext)s",
                Cookies = cookiesPath
            };
            
            options.AddCustomOption("--ppa", PostProcessing);
            return await client.RunWithOptions(url, options, progress: progress, ct: token, output: Logger);
        }
    }
}