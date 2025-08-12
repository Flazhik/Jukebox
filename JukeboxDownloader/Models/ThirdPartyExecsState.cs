using JetBrains.Annotations;

namespace JukeboxDownloader.Models
{
    public class ThirdPartyExecsState
    {
        public ThirdPartyExecsState()
        {
            state = ExecsState.None;
        }

        public ExecsState state;
        
        public float? progress;
        
        [CanBeNull]
        public string currentBinary;

        public static ThirdPartyExecsState DownloadingStarted() =>
            new()
            {
                state = ExecsState.Downloading,
                currentBinary = null,
                progress = null
            };
        
        public static ThirdPartyExecsState DownloadingProgress(string software, float progress) =>
            new()
            {
                state = ExecsState.Downloading,
                currentBinary = software,
                progress = progress
            };
        
        public static ThirdPartyExecsState Failed() =>
            new()
            {
                state = ExecsState.Failed,
                currentBinary = null,
                progress = null
            };
        
        public static ThirdPartyExecsState Present() =>
            new()
            {
                state = ExecsState.Present,
                currentBinary = null,
                progress = null
            };
    }

    public enum ExecsState
    {
        None,
        Missing,
        Present,
        Downloading,
        Failed
    }
}