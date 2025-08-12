using JukeboxCore.Models.Downloader;
using UnityEngine;

namespace JukeboxDownloader.Events
{
    public class ThumbnailLoadedEvent : DownloadableEntityStateEvent
    {
        public ThumbnailLoadedEvent(Sprite sprite) : base(GetState(sprite))
        {
        }

        private static DownloadableEntityState GetState(Sprite sprite) => new() { thumbnail = sprite };
        
        public override void Mutate(DownloadableEntityState state)
        {
            state.thumbnail = newState.thumbnail;
        }
    }
}