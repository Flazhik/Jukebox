namespace Jukebox.Utils
{
    public static class FloatExtensions
    {
        public static string SecondsToHumanReadable(this float value)
        {
            var integer = (int)value;
            var minutes = integer / 60;
            var seconds = integer - minutes * 60;
            return $"{minutes:00}:{seconds:00}";
        }
    }
}