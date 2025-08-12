using System.Linq;

namespace JukeboxCore.Utils
{
    public static class GenericUtils
    {
        public static string FirstNonEmpty(params string[] options) =>
            options.FirstOrDefault(option => !string.IsNullOrEmpty(option));
    }
}