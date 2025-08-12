using JukeboxCore.Models.Downloader;

namespace JukeboxDownloader.Events
{
    public class DownloadFailedEvent : DownloadableEntityStateEvent
    {
        public DownloadFailedEvent() : base(GetState())
        {
        }

        private static DownloadableEntityState GetState() => new() { state = TrackDownloadingState.Failed };
        
        public override void Mutate(DownloadableEntityState state)
        {
            state.state = newState.state;
        }
    }
}