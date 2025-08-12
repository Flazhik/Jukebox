using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Jukebox.Utils;
using TMPro;
using UnityEngine;

namespace Jukebox.UI
{
    public class JukeboxCredits: MonoBehaviour
    {
        [SerializeField]
        private TMP_Text[] terminalText;

        [SerializeField]
        private AudioSource click;

        [SerializeField]
        private GameObject firstWindow;
        
        [SerializeField]
        private GameObject secondWindow;
        
        [SerializeField]
        private List<GameObject> lastWindows;

        private string[] originalText;

        private void Awake()
        {
            originalText = terminalText.Select(t => t.text).ToArray();
        }

        private void OnEnable()
        {
            firstWindow.SetActive(false);
            secondWindow.SetActive(false);
            foreach (var window in lastWindows)
                window.SetActive(false);
            foreach (var tmpText in terminalText)
                tmpText.text = string.Empty;
            
            StartCoroutine(WindowsRoutine());
        }

        private IEnumerator WindowsRoutine()
        {
            yield return TerminalTextRoutine(0);
            firstWindow.SetActive(true);
            
            yield return TerminalTextRoutine(1);
            yield return TerminalTextRoutine(2);
            secondWindow.SetActive(true);
            
            yield return TerminalTextRoutine(3);
            yield return TerminalTextRoutine(4);
            foreach (var window in lastWindows)
                window.SetActive(true);
        }

        private IEnumerator TerminalTextRoutine(int index) =>
            ScrollingTextUnscaled.ShowText(terminalText[index], originalText[index], 0.01f, click);
    }
}