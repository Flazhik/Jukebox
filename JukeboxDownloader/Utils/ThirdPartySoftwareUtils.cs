using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static Newtonsoft.Json.JsonConvert;
using static JukeboxCore.Utils.NetworkUtils;
using static JukeboxCore.Utils.PathsUtils;
using static YoutubeDLSharp.Utils;

namespace JukeboxDownloader.Utils
{
    public static class ThirdPartySoftwareUtils
    {
        private const string FfmpegApiUrl = "https://ffbinaries.com/api/v1/version/6.1";
        private const string YtDlpUrl = "https://github.com/yt-dlp/yt-dlp/releases/download/2025.12.08/yt-dlp.exe";

        private static readonly List<string> BinariesNames = new()
        {
            YtDlpBinaryName,
            FfmpegBinaryName,
            FfprobeBinaryName
        };

        private static readonly Dictionary<string, string> BinariesHashes = new()
        {
            { "ffmpeg.exe", "7afdd1037ac16bce65a7bb721818cc9fa0857059" },
            { "ffprobe.exe", "678fcc1327c5d86c33e1f2e463bcb592f6c1643f" },
            { "yt-dlp.exe", "5329549c5611209c1e7bcbc441d95db6184adc23" }
        };

        private static readonly SHA1CryptoServiceProvider SHA1Provider = new();

        public static bool AllRequiredSoftwarePresent() => BinariesNames.All(ThirdPartySoftwarePresent);

        public static bool ValidateThirdPartySoftwareIntegrity() => BinariesNames.All(ValidateBinaryIntegrity);

        public static bool ThirdPartySoftwarePresent(string binary)
        {
            var file = new FileInfo(Path.Combine(AssemblyPath, binary));
            return file.Exists && BinariesHashes.ContainsKey(binary);
        }

        public static bool ValidateBinaryIntegrity(string binary)
        {
            var file = new FileInfo(Path.Combine(AssemblyPath, binary));
            return file.Exists && GetFileHash(file.FullName).Equals(BinariesHashes[binary]);
        }

        public static async Task DownloadYtDlp(WebClient client)
        {
            var path = Path.Combine(AssemblyPath, YtDlpBinaryName);
            if (File.Exists(path))
                File.Delete(path);
            await client.DownloadFileTaskAsync(new Uri(YtDlpUrl), path);
        }

        public static async Task DownloadFfmpeg(WebClient client) =>
            await DownloadFf(client, downloader => downloader.Ffmpeg, "ffmpeg.exe");

        public static async Task DownloadFfprobe(WebClient client) =>
            await DownloadFf(client, downloader => downloader.Ffprobe, "ffprobe.exe");

        private static async Task DownloadFf(WebClient client, Func<FfmpegApi.OsBinVersion, string> url,
            string fileName)
        {
            var zipPath = Path.Combine(AssemblyPath, fileName);
            var downloader = await FfDownloader();
            if (File.Exists(zipPath))
                File.Delete(zipPath);
            await client.DownloadFileTaskAsync(new Uri(url(downloader)), zipPath);
            await Task.Run(() => Unzip(zipPath));
        }

        public static void CleanUp()
        {
            foreach (var zipFile in new DirectoryInfo(AssemblyPath)
                         .EnumerateFiles()
                         .Where(file => file.Extension.ToLower().Equals(".zip")))
                zipFile.Delete();
        }

        private static async Task<FfmpegApi.OsBinVersion> FfDownloader()
        {
            var ffmpegVersion = DeserializeObject<FfmpegApi.Root>(await GetRaw(FfmpegApiUrl));
            return ffmpegVersion?.Bin.Windows64;
        }

        private static void Unzip(string path)
        {
            using var stream = new MemoryStream(File.ReadAllBytes(path));
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
            if (archive.Entries.Count <= 0)
                return;

            var outputPath = Path.Combine(AssemblyPath, archive.Entries[0].FullName);
            if (File.Exists(outputPath))
                File.Delete(outputPath);
            archive.Entries[0].ExtractToFile(Path.Combine(AssemblyPath, archive.Entries[0].FullName), true);
        }

        private static string GetFileHash(string path)
        {
            using var rs = File.OpenRead(path);
            var rawHash = SHA1Provider.ComputeHash(rs);
            var sb = new StringBuilder(rawHash.Length * 2);
            foreach (var b in rawHash)
                sb.Append(b.ToString("x2"));

            return sb.ToString();
        }

        // I couldn't figure out the way to do it without copying all this stuff from YoutubeDLSharp

        #region FFmpeg API JSON model

        internal class FfmpegApi
        {
            public class Root
            {
                [JsonProperty("version")] public string Version { get; set; }

                [JsonProperty("permalink")] public string Permalink { get; set; }

                [JsonProperty("bin")] public Bin Bin { get; set; }
            }

            public class Bin
            {
                [JsonProperty("windows-64")] public OsBinVersion Windows64 { get; set; }
            }

            public class OsBinVersion
            {
                [JsonProperty("ffmpeg")] public string Ffmpeg { get; set; }

                [JsonProperty("ffprobe")] public string Ffprobe { get; set; }
            }
        }

        #endregion
    }
}