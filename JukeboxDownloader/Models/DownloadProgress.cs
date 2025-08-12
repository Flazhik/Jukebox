using JukeboxCore.Models.Downloader;

namespace JukeboxDownloader.Models
{
    public class DownloadProgress
    {
        public TrackDownloadingState State { get; private set; }
        public float Progress { get; private set; }
        public string Eta { get; private set; }

        public DownloadProgress(TrackDownloadingState state, float progress, string eta)
        {
            State = state;
            Progress = progress;
            Eta = eta;
        }
    }
}