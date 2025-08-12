using JukeboxCore.Models.Downloader;

namespace JukeboxDownloader.Events
{
    public class TrackEnqueuedEvent : DownloadableEntityStateEvent
    {
        public TrackEnqueuedEvent() : base(GetState())
        {
        }

        private static DownloadableEntityState GetState() => new() { state = TrackDownloadingState.Enqueued };
        
        public override void Mutate(DownloadableEntityState state)
        {
            state.state = newState.state;
        }
    }
}