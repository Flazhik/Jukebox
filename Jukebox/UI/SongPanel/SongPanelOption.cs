using UnityEngine;
using UnityEngine.UI;

namespace Jukebox.UI.SongPanel
{
    public class SongPanelOption: MonoBehaviour
    {
        private static readonly Color Active = new(0.68f, 0.68f, 0.68f, 0.25f);
        private static readonly Color Inactive = new(0, 0, 0, 0.59f);
        
        public SongPanelStyle style;

        private void Start()
        {
            gameObject.GetComponent<Image>().color =
                PrefsManager.Instance.GetIntLocal("jukebox.songPanelStyle") == (int)style ? Active : Inactive;
        }

        public void Select()
        {
            foreach (var b in transform.parent.GetComponentsInChildren<Button>())
                b.gameObject.GetComponent<Image>().color = b == gameObject.GetComponent<Button>() ? Active : Inactive;
                
            PrefsManager.Instance.SetIntLocal("jukebox.songPanelStyle", (int)style);
        }
    }
}