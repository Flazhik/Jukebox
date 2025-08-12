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

namespace JukeboxDownloader.Service.YouTube
{
    public class YoutubeClient : AbstractDownloaderClient
    {
        private const string YouTubeVideoRegex =
            "^(?:https:\\/\\/)?(?:(?:www|m)\\.)?(?:youtube\\.com|youtu.be)(?:\\/(?:[\\w\\-]+\\?v=|embed\\/|v\\/)?)([\\w\\-]+)(\\S+)?$";
        private const string YouTubePlaylistRegex =
            "^(?:https:\\/\\/)?(?:(?:www|m)\\.)?(?:youtube\\.com|youtu.be).*?list=([a-zA-Z0-9\\-_]*).*(?:&|$)$";
        
        private readonly YtDlpClient client;

        public YoutubeClient()
        {
            client = new YtDlpClient();
        }

        public override bool SupportsUrl(string url) => UrlIsSingleTrack(url) || UrlIsPlaylist(url);

        public override async Task<List<DownloadableSongMetadata>> GetMetadata(string url, CancellationToken token)
        {
            var targetUrl = url;
            
            // URLs such as /watch?v=videoId&list=listId must be treated as playlists
            if (UrlIsPlaylist(url))
                targetUrl = ConstructPlaylistUrl(GetPlaylistId(url));
            
            var data = await client.GetMetadata(targetUrl, token);
            if (!data.Success || data.Data == null)
                throw HandleErrorOutput(data.ErrorOutput);

            var isPlaylist = data.Data.Entries != null && data.Data.Entries.Length != 0;
            var entries = isPlaylist
                ? data.Data.Entries
                : new [] { data.Data };
            
            return entries
                .Select(entry => new DownloadableSongMetadata(
                    entry.ID,
                    isPlaylist ? entry.Url : targetUrl,
                    $"https://i.ytimg.com/vi/{entry.ID}/hqdefault.jpg",
                    entry.Title,
                    entry.Uploader,
                    isPlaylist ? data.Data.Title : default))
                .ToList();
        }

        public override async Task Download(
            string url,
            string playlist,
            IProgress<DownloadProgress> progress,
            CancellationToken token)
        {
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

            progress.Report(result.Success
                ? new DownloadProgress(TrackDownloadingState.Success, 1f, string.Empty)
                : new DownloadProgress(TrackDownloadingState.Failed, 0f, string.Empty));
        }

        private static bool UrlIsSingleTrack(string url) =>
            Regex.IsMatch(url, YouTubeVideoRegex, RegexOptions.IgnoreCase);

        private static bool UrlIsPlaylist(string url) =>
            Regex.IsMatch(url, YouTubePlaylistRegex, RegexOptions.IgnoreCase);
        
        private static string GetPlaylistId(string url) =>
            Regex.Match(url, YouTubePlaylistRegex).Groups[1].Value;
        
        private static string ConstructPlaylistUrl(string playlistId) =>
            "https://youtube.com/playlist?list=" + playlistId;

        private static Exception HandleErrorOutput(string[] errorOutput)
        {
            var output = string.Join("\n", errorOutput);
            return Handle(output,
                (new [] { "Sign in to confirm you" }, () => new CookiesRequiredException()),
                (new [] { "Incomplete YouTube ID", "Video unavailable" }, () => new InvalidVideoUrlException()),
                (new [] { "skipping cookie file entry" }, () => new InvalidCookiesFileException()),
                (new [] { "This playlist type is unviewable" }, () => new PlaylistIsUnviewableException()));

            Exception Handle(string o, params (string[], Func<JukeboxException>)[] handlers)
            {
                var exception = handlers
                    .FirstOrDefault(handler => handler.Item1
                        .FirstOrDefault(o.Contains) != default);
                
                if (exception != default)
                    throw exception.Item2();
                
                return new UnableToFetchMetadata(output);
            }
        }
    }
}