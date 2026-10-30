using Jukebox.RTLTMPro;
using TMPro;

namespace JukeboxDownloader.Utils;

public static class TmpTextExtensions
{
    public static void SetPreservingRtl(this TMP_Text text, string value) => RtlUtils.Set(text, value);
}