using System.Collections;
using Jukebox.Components;
using JukeboxCore.Playback;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Jukebox.UI.Subtitles;

public class SubtitlesPanel : MonoBehaviour
{
    [SerializeField] public CanvasGroup panelGroup;
    [SerializeField] public TMP_Text text;
    [SerializeField] public float approachTime;
    [SerializeField] public Material fontMaterial;

    private bool displaySubtitles;
    private Coroutine displayRoutine;
    private IPlaybackController player;

    public void Awake()
    {
        player = JukeboxManager.Instance?.player;
        if (player == null)
        {
            enabled = false;
            return;
        }

        PrefsManager.onPrefChanged += OnPrefChanged;
        player.OnSubtitleChanged += OnSubtitleChanged;
        player.OnStop += OnStop;
        displaySubtitles = PrefsManager.Instance!.GetBoolLocal("jukebox.displaySubtitles");
        Stall();
    }

    public void OnDestroy()
    {
        PrefsManager.onPrefChanged -= OnPrefChanged;
        
        if (player == null)
            return;
        
        player.OnSubtitleChanged -= OnSubtitleChanged;
        player.OnStop -= OnStop;
    }

    public void RestoreFontMaterial()
    {
        if (fontMaterial != null)
            text.fontMaterial = fontMaterial;
    }

    private void OnSubtitleChanged(string contents)
    {
        if (!displaySubtitles)
            return;

        if (string.IsNullOrEmpty(contents))
        {
            StartFade(0f);
            return;
        }

        text.text = contents;
        StartFade(1f);
        
        text.ForceMeshUpdate();
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent.GetComponent<RectTransform>());
    }

    private void OnStop()
    {
        Stall();
    }

    private void OnPrefChanged(string key, object value)
    {
        if (!key.Equals("jukebox.displaySubtitles"))
            return;

        displaySubtitles = (bool)value;
        if (!displaySubtitles)
            Stall();
    }

    private void StartFade(float target)
    {
        if (displayRoutine != null)
            StopCoroutine(displayRoutine);

        displayRoutine = null;

        if (Mathf.Approximately(panelGroup.alpha, target))
            return;

        displayRoutine = StartCoroutine(Fade(target));
    }

    private IEnumerator Fade(float target)
    {
        var start = panelGroup.alpha;
        var time = 0f;

        while (time < approachTime)
        {
            time += Time.unscaledDeltaTime;
            panelGroup.alpha = Mathf.Lerp(start, target, time / approachTime);
            yield return null;
        }

        panelGroup.alpha = target;
    }

    private void Stall()
    {
        if (displayRoutine != null)
        {
            StopCoroutine(displayRoutine);
            displayRoutine = null;
        }

        text.text = string.Empty;
        panelGroup.alpha = 0f;
    }
}
