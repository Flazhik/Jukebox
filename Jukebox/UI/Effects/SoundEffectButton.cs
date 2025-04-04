using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Jukebox.UI.Effects
{
    public class SoundEffectButton : MonoBehaviour
    {
        [SerializeField]
        private string prefsKey;
        
        [SerializeField]
        private TMP_Text title;

        [SerializeField]
        private TMP_Text fileNameText;

        [SerializeField]
        private TMP_Text browserWindowTitle;

        protected void Awake()
        {
            ChangeFileName(PrefsManager.Instance.GetStringLocal(prefsKey));
            PrefsManager.onPrefChanged += OnPrefChanged;
            GetComponent<Button>().onClick.AddListener(() => browserWindowTitle.text = $"Select file for {title.text}");
        }

        protected void OnDestroy()
        {
            PrefsManager.onPrefChanged -= OnPrefChanged;
        }
        
        private void OnPrefChanged(string key, object value)
        {
            if (!key.Equals(prefsKey))
                return;

            ChangeFileName((string)value);
        }
        
        private void ChangeFileName(string fileName) =>
            fileNameText.text = fileName != null
                ? new FileInfo(fileName).Name
                : "Default";
    }
}