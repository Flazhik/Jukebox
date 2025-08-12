using JetBrains.Annotations;

namespace JukeboxCore.Models.Downloader
{
    public class DownloadableSongMetadata
    {
        public string Id { get; private set; }
        public string Url { get; private set; }
        public string ThumbnailUrl { get; private set; }
        public string Title { get; private set; }
        public string Artist { get; private set; }
        [CanBeNull] public string Playlist { get; private set; }

        public DownloadableSongMetadata(string id, string url, string thumbnailUrl, string title, string artist, string playlist)
        {
            Id = id;
            Url = url;
            ThumbnailUrl = thumbnailUrl;
            Title = title;
            Artist = artist;
            Playlist = playlist;
        }
    }
}