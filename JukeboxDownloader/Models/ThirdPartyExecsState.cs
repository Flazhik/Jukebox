using JetBrains.Annotations;

namespace JukeboxDownloader.Models;

public class ThirdPartyExecsState
{
    [CanBeNull] public string currentBinary;
    public float? progress;
    public ExecsState state = ExecsState.None;

    public static ThirdPartyExecsState DownloadingStarted()
    {
        return new ThirdPartyExecsState
        {
            state = ExecsState.Downloading,
            currentBinary = null,
            progress = null
        };
    }

    public static ThirdPartyExecsState DownloadingProgress(string software, float progress)
    {
        return new ThirdPartyExecsState
        {
            state = ExecsState.Downloading,
            currentBinary = software,
            progress = progress
        };
    }

    public static ThirdPartyExecsState Failed()
    {
        return new ThirdPartyExecsState
        {
            state = ExecsState.Failed,
            currentBinary = null,
            progress = null
        };
    }

    public static ThirdPartyExecsState Present()
    {
        return new ThirdPartyExecsState
        {
            state = ExecsState.Present,
            currentBinary = null,
            progress = null
        };
    }
}

public enum ExecsState
{
    None,
    Missing,
    Present,
    UpdateRequired,
    Downloading,
    Failed
}