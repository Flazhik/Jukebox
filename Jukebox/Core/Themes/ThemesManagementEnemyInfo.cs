using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Jukebox.Core.Themes
{
    [CreateAssetMenu(menuName = "Jukebox/Enemy info")]
    public class ThemesManagementEnemyInfo: ScriptableObject
    {
        public AssetReferenceT<Sprite> icon;
        public string enemyName;
        public EnemyType enemyType;
    }
}