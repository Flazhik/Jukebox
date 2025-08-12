using System.Reflection;
using JukeboxCore.Assets;
using JukeboxCore.Themes;
using UnityEngine;

namespace JukeboxCore.Components
{
    [ConfigureSingleton(SingletonFlags.NoAutoInstance)]
    public class JukeboxStaticData : MonoSingleton<JukeboxStaticData>
    {
        [SerializeField] public SoundtrackCalmThemes calmThemes;

        protected new void Awake()
        {
            base.Awake();
            AssetsManager.Instance.LoadAssets(Resources.JukeboxCore);
            AssetsManager.Instance.RegisterPrefabs(Assembly.GetExecutingAssembly());
        }
    }
}