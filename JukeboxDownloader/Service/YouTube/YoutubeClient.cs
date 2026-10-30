using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using JukeboxCore.Exceptions;
using JukeboxCore.Models.Downloader;
using JukeboxDownloader.Exceptions;
using YoutubeDLSharp;
using DownloadProgress = JukeboxDownloader.Models.DownloadProgress;
using static JukeboxCore.Logging.JukeboxLogger;

namespace JukeboxDownloader.Service.YouTube;

public class YoutubeClient : AbstractDownloaderClient
{
    private const string YouTubeVideoRegex =
        @"^(?:https:\/\/)?(?:(?:www|m)\.)?(?:youtube\.com|youtu.be)(?:\/(?:[\w\-]+\?v=|embed\/|v\/)?)([\w\-]+)(\S+)?$";
    private const string YouTubePlaylistRegex =
        @"^(?:https:\/\/)?(?:(?:www|m)\.)?(?:youtube\.com|youtu.be).*?list=(PL[a-zA-Z0-9\-_]*).*(?:&|$)$";

    private readonly YtDlpClient client = new();

    public override bool SupportsUrl(string url)
    {
        return UrlIsSingleTrack(url) || UrlIsPlaylist(url);
    }

    public override async Task<List<DownloadableSongMetadata>> GetMetadata(string url, CancellationToken token)
    {
        /*
         * URLs such as /watch?v=videoId&list=listId must be treated as a playlist
         * However, we must treat is as a single track when the playlist id doesn't start with PL
         */
        var targetUrl = UrlIsPlaylist(url)
            ? ConstructPlaylistUrl(GetPlaylistId(url))
            : ConstructVideoUrl(GetVideoId(url));

        var data = await client.GetMetadata(targetUrl, token);
        if (!data.Success || data.Data == null)
            throw HandleErrorOutput(data.ErrorOutput);

        var isPlaylist = data.Data.Entries != null && data.Data.Entries.Length != 0;
        var entries = isPlaylist
            ? data.Data.Entries
            : [data.Data];

        return entries
            .Select(entry => new DownloadableSongMetadata(
                entry.ID,
                isPlaylist ? entry.Url : targetUrl,
                $"https://i.ytimg.com/vi/{entry.ID}/hqdefault.jpg",
                entry.Title,
                entry.Uploader,
                isPlaylist ? data.Data.Title : null))
            .ToList();
    }

    public override async Task Download(
        string url,
        string playlist,
        IProgress<DownloadProgress> progress,
        CancellationToken token)
    {
        Log.LogInfo($"Starting YouTube download for URL: {url}");
        IProgress<YoutubeDLSharp.DownloadProgress> downloadProcess =
            new Progress<YoutubeDLSharp.DownloadProgress>(p =>
            {
                var downloadState = p.State switch
                {
                    DownloadState.None => TrackDownloadingState.None,
                    DownloadState.PreProcessing => TrackDownloadingState.PreProcessing,
                    DownloadState.Downloading => TrackDownloadingState.Downloading,
                    DownloadState.PostProcessing => TrackDownloadingState.PostProcessing,
                    DownloadState.Success => TrackDownloadingState.Success,
                    DownloadState.Error => TrackDownloadingState.PreProcessing, // Had to be done this way
                    _ => TrackDownloadingState.None
                };
                progress.Report(new DownloadProgress(downloadState, p.Progress, p.ETA));
            });

        var result = await client.Download(url, playlist, downloadProcess, token);
        if (!result.Success)
        {
            var err = result.ErrorOutput != null ? string.Join("; ", result.ErrorOutput) : "Unknown error";
            Log.LogError($"YouTube download failed for {url}: {err}");
        }

        progress.Report(result.Success
            ? new DownloadProgress(TrackDownloadingState.Success, 1f, string.Empty)
            : new DownloadProgress(TrackDownloadingState.Failed, 0f, string.Empty));
    }

    private static bool UrlIsSingleTrack(string url) => Regex.IsMatch(url, YouTubeVideoRegex, RegexOptions.IgnoreCase);
    
    private static bool UrlIsPlaylist(string url) => Regex.IsMatch(url, YouTubePlaylistRegex, RegexOptions.IgnoreCase);

    private static string GetPlaylistId(string url) => Regex.Match(url, YouTubePlaylistRegex).Groups[1].Value;
    
    private static string GetVideoId(string url) => Regex.Match(url, YouTubeVideoRegex).Groups[1].Value;

    private static string ConstructPlaylistUrl(string playlistId) => "https://youtube.com/playlist?list=" + playlistId;
    
    private static string ConstructVideoUrl(string videoId) => "https://youtube.com/watch?v=" + videoId;
    
    private static Exception HandleErrorOutput(string[] errorOutput)
    {
        var output = string.Join("\n", errorOutput);
        return Handle(output,
            (["Sign in to confirm you"], static () => new CookiesRequiredException()),
            (["Incomplete YouTube ID", "Video unavailable"], static () => new InvalidVideoUrlException()),
            (["skipping cookie file entry"], static () => new InvalidCookiesFileException()),
            (["This playlist type is unviewable"], static () => new PlaylistIsUnviewableException())
            //(["Ensure you have a supported JavaScript runtime"], static () => new JsRuntimeMissingException())
            );

        Exception Handle(string o, params (string[], Func<JukeboxException>)[] handlers)
        {
            var exception = handlers
                .FirstOrDefault(handler => handler.Item1
                    .FirstOrDefault(o.Contains) != null);

            return exception != default ? throw exception.Item2() : new UnableToFetchMetadata(output);
        }
    }
}