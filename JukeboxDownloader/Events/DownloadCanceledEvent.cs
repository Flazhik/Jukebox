using JukeboxCore.Models.Downloader;

namespace JukeboxDownloader.Events
{
    public class DownloadCanceledEvent : DownloadableEntityStateEvent
    {
        public DownloadCanceledEvent() : base(GetState())
        {
        }

        private static DownloadableEntityState GetState() => new() { state = TrackDownloadingState.None };
        
        public override void Mutate(DownloadableEntityState state)
        {
            state.state = newState.state;
        }
    }
}