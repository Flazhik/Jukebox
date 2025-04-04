using System.Collections;
using System.Collections.Generic;
using Jukebox.Utils;
using TMPro;
using UnityEngine;

namespace Jukebox.UI
{
    public class JukeboxCredits: MonoBehaviour
    {
        [SerializeField]
        private TMP_Text terminalText;

        [SerializeField]
        private AudioSource click;

        [SerializeField]
        private GameObject firstWindow;
        
        [SerializeField]
        private GameObject secondWindow;
        
        [SerializeField]
        private List<GameObject> lastWindows;

        private string originalText;

        private void Awake()
        {
            originalText = terminalText.text;
        }

        private void OnEnable()
        {
            firstWindow.SetActive(false);
            secondWindow.SetActive(false);
            foreach (var window in lastWindows)
                window.SetActive(false);
            
            StartCoroutine(ScrollingTextUnscaled.ShowText(terminalText, originalText, 0.01f, click));
            StartCoroutine(WindowsRoutine());
        }

        private IEnumerator WindowsRoutine()
        {
            yield return new WaitForSecondsRealtime(1f);
            firstWindow.SetActive(true);
            yield return new WaitForSecondsRealtime(1f);
            secondWindow.SetActive(true);
            yield return new WaitForSecondsRealtime(2f);
            foreach (var window in lastWindows)
                window.SetActive(true);
        }
    }
}