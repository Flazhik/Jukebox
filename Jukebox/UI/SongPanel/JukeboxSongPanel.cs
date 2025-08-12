using System.Collections;
using Jukebox.Utils;
using JukeboxCore.Models.Song;
using UnityEngine;

namespace Jukebox.UI.SongPanel
{
    public class JukeboxSongPanel: SongPanel
    {
        protected override SongPanelStyle Style => SongPanelStyle.Jukebox;
        
        [SerializeField]
        public Vector2 startDimensions;

        [SerializeField]
        public Vector2 targetDimensions;

        private Coroutine rollingTextRoutine;

        protected override IEnumerator FadeIn(JukeboxSongMetadata metadata)
        {
            text.color = new Color(1, 1, 1, 1);
            var rect = panelGroup.GetComponent<RectTransform>();
            rect.sizeDelta = startDimensions;

            var artist = !string.IsNullOrEmpty(metadata.Artist)
                ? $"<color=#999>{metadata.Artist}</color>"
                : string.Empty;
            
            if (rollingTextRoutine != null)
                StopCoroutine(rollingTextRoutine);
            text.text = string.Empty;
            rollingTextRoutine = StartCoroutine(ScrollingTextUnscaled.ShowText(text, $"{metadata.Title.ToUpper()} {artist}", 0.022f));
            var time = 0.0f;
            while (time < panelApproachTime)
            {
                panelGroup.alpha = active ? 1 : 0;
                time += Time.unscaledDeltaTime;
                
                if (time > panelApproachTime)
                    time = panelApproachTime;
                
                rect.sizeDelta = startDimensions + (targetDimensions - startDimensions) * time / panelApproachTime;
                yield return null;
            }
        }

        protected override IEnumerator Await()
        {
            var time = panelStayTime;
            while (time > 0.0 || showIndefinitely)
            {
                panelGroup.alpha = active ? 1 : 0;
                if (time > 0.0) 
                    time -= Time.unscaledDeltaTime;
                yield return null;
            }
        }

        protected override IEnumerator FadeOut()
        {
            rollingTextRoutine = StartCoroutine(ScrollingTextUnscaled.HideText(text, 0.022f));
            var rect = panelGroup.GetComponent<RectTransform>();
            var time = panelApproachTime;
            yield return new WaitUntil(() => text.text.Length <= 20);
            while (time > 0.0)
            {
                panelGroup.alpha = active ? 1 : 0;
                time -= Time.unscaledDeltaTime;
                
                if (time < 0)
                    time = 0;
                
                rect.sizeDelta = startDimensions + (targetDimensions - startDimensions) * time / panelApproachTime;
                yield return null;
            }
            panelGroup.alpha = 0;
        }
    }
}