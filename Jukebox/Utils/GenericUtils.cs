using System.Linq;

namespace Jukebox.Utils
{
    public class GenericUtils
    {
        public static string FirstNonEmpty(params string[] options) =>
            options.FirstOrDefault(option => !string.IsNullOrEmpty(option));
    }
}