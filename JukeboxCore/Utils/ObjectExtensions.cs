using System.Linq;

namespace JukeboxCore.Utils
{
    public static class ObjectExtensions
    {
        public static bool IsOneOf(this object v, params object[] options) => options.Any(o => o.Equals(v));
    }
}