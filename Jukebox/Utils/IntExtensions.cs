namespace Jukebox.Utils
{
    public static class IntExtensions
    {
        public static int Mod(this int num, int mod) => (num % mod + mod) % mod;
    }
}