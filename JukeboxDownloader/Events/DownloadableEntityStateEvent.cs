using JukeboxCore.Events;
using JukeboxCore.Models.Downloader;

namespace JukeboxDownloader.Events
{
    public abstract class DownloadableEntityStateEvent : JukeboxEvent<DownloadableEntityState>
    {
        protected DownloadableEntityStateEvent(DownloadableEntityState newState)
        {
            this.newState = newState;
        }
    }
}