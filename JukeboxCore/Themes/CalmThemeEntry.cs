using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace JukeboxCore.Themes
{
    [CreateAssetMenu(menuName = "Jukebox/Calm theme")]
    public class CalmThemeEntry: ScriptableObject
    {
        [SerializeField]
        public AssetReferenceSoundtrackSong reference;
        
        [SerializeField]
        public List<AssetReferenceT<AudioClip>> calmVariationClips;
    }
}