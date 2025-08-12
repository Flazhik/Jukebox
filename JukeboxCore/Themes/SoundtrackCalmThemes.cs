using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace JukeboxCore.Themes
{
    [CreateAssetMenu(menuName = "Jukebox/Calm Themes Data")]
    public class SoundtrackCalmThemes: ScriptableObject
    {
        [SerializeField]
        public CalmThemeEntry[] calmClips;

        public List<AssetReferenceT<AudioClip>> FindCalmClipsFor(string guid)
        {
            var entry = calmClips.FirstOrDefault(variation =>
                variation.reference.AssetGUID == guid);

            return entry != default ? entry.calmVariationClips : default;
        }
    }
}