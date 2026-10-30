using JetBrains.Annotations;

namespace JukeboxCore.Models.Downloader;

public class DownloadableSongMetadata(
    string id,
    string url,
    string thumbnailUrl,
    string title,
    string artist,
    string playlist)
{
    public string Id { get; private set; } = id;
    public string Url { get; private set; } = url;
    public string ThumbnailUrl { get; private set; } = thumbnailUrl;
    public string Title { get; private set; } = title;
    public string Artist { get; private set; } = artist;
    [CanBeNull] public string Playlist { get; private set; } = playlist;
}