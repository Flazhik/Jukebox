using JetBrains.Annotations;
using UnityEngine;

namespace JukeboxCore.Models.Downloader;

public class DownloadableEntityState
{
    public float? progress;
    public TrackDownloadingState? state;
    [CanBeNull] public Sprite thumbnail;
    [CanBeNull] public string eta;
}