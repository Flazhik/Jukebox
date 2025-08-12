using JetBrains.Annotations;
using UnityEngine;

namespace JukeboxCore.Models.Downloader
{
    public class DownloadableEntityState
    {
        [CanBeNull]
        public Sprite thumbnail;
        
        public TrackDownloadingState? state;
        
        public float? progress;
        
        [CanBeNull]
        public string eta;
    }
}