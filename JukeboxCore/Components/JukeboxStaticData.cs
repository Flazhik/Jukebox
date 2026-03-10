using JukeboxCore.Themes;
using UnityEngine;

namespace JukeboxCore.Components
{
    [ConfigureSingleton(SingletonFlags.NoAutoInstance)]
    public class JukeboxStaticData : MonoSingleton<JukeboxStaticData>
    {
        [SerializeField] public SoundtrackCalmThemes calmThemes;
    }
}