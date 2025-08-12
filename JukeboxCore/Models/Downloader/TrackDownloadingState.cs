namespace JukeboxCore.Models.Downloader
{
    public enum TrackDownloadingState
    {
        None,
        Enqueued,
        PreProcessing,
        Downloading,
        PostProcessing,
        Success,
        Failed
    }
}