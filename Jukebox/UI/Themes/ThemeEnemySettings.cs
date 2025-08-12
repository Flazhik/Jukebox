using Jukebox.UI.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Jukebox.UI.Themes
{
    public class ThemeEnemySettings: MonoBehaviour
    {
        [SerializeField]
        public TMP_Text title;
        
        [SerializeField]
        public Image icon;
        
        [SerializeField]
        public GameObject radianceBg;
        
        [SerializeField]
        public Toggle toggle;
        
        [SerializeField]
        public Counter counter;
    }
}