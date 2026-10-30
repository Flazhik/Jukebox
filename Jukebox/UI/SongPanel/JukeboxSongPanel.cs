using System.Collections;
using Jukebox.Utils;
using JukeboxCore.Songs.Models;
using UnityEngine;

namespace Jukebox.UI.SongPanel;

public class JukeboxSongPanel : SongPanel
{
    [SerializeField] public Vector2 startDimensions;
    [SerializeField] public Vector2 targetDimensions;

    private Coroutine rollingTextRoutine;

    protected override SongPanelStyle Style => SongPanelStyle.Jukebox;

    protected override IEnumerator FadeIn(JukeboxSongMetadata metadata)
    {
        text.color = new Color(1, 1, 1, 1);
        var rect = panelGroup.GetComponent<RectTransform>();
        rect.sizeDelta = startDimensions;

        if (rollingTextRoutine != null)
            StopCoroutine(rollingTextRoutine);
        rollingTextRoutine = StartCoroutine(ScrollingTextUnscaled.Reveal(text, 0.022f));
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
        rollingTextRoutine = StartCoroutine(ScrollingTextUnscaled.Conceal(text, 0.022f));
        var rect = panelGroup.GetComponent<RectTransform>();
        var time = panelApproachTime;
        yield return new WaitUntil(() => text.maxVisibleCharacters <= 20);
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
