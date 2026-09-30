using JukeboxCore.Models.Downloader;

namespace JukeboxDownloader.Models;

public class DownloadProgress(TrackDownloadingState state, float progress, string eta)
{
    public TrackDownloadingState State { get; private set; } = state;
    public float Progress { get; private set; } = progress;
    public string Eta { get; private set; } = eta;
}