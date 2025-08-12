using JukeboxCore.Models.Downloader;

namespace JukeboxDownloader.Events
{
    public class ProgressUpdatedEvent : DownloadableEntityStateEvent
    {
        public ProgressUpdatedEvent(TrackDownloadingState state, float progress, string eta)
            : base(GetState(state, progress, eta))
        {
        }

        private static DownloadableEntityState GetState(
            TrackDownloadingState newState,
            float newProgress,
            string eta) =>
            new()
            {
                state = newState,
                progress = newProgress,
                eta = eta
            };
        
        public override void Mutate(DownloadableEntityState state)
        {
            state.state = newState.state;
            state.progress = newState.progress;
        }
    }
}