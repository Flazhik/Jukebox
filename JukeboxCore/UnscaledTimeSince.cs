using UnityEngine;

namespace JukeboxCore
{
    public struct UnscaledTimeSince
    {
        private float time;

        public static implicit operator float(UnscaledTimeSince ts) => Time.unscaledTime - ts.time;

        public static implicit operator UnscaledTimeSince(float ts) => new()
        {
            time = Time.unscaledTime - ts
        };
    }
}