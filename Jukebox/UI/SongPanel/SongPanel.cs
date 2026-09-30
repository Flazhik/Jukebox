using System.Collections;
using Jukebox.Components;
using JukeboxCore.Events;
using JukeboxCore.Playback;
using JukeboxCore.Songs.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Jukebox.UI.SongPanel;

public abstract class SongPanel : MonoBehaviour
{
    [SerializeField] public CanvasGroup panelGroup;
    [SerializeField] public Sprite defaultIcon;
    [SerializeField] public float panelApproachTime;
    [SerializeField] public float panelStayTime;

    public TMP_Text text;
    public Image icon;

    protected bool active;
    protected bool showIndefinitely;
    private Coroutine displayRoutine;
    private IPlaybackController player;

    protected abstract SongPanelStyle Style { get; }

    public void Awake()
    {
        PrefsManager.onPrefChanged += OnPrefChanged;
        player = JukeboxManager.Instance!.player;
        player.OnNextAudioClip += NextAudioClip;
        player.OnStop += OnStop;
        showIndefinitely = PrefsManager.Instance.GetBoolLocal("jukebox.showTrackPanelIndefinitely");
        active = PrefsManager.Instance.GetIntLocal("jukebox.songPanelStyle") == (int)Style;
    }

    public void OnDestroy()
    {
        PrefsManager.onPrefChanged -= OnPrefChanged;
        player.OnNextAudioClip -= NextAudioClip;
        player.OnStop -= OnStop;
    }

    protected abstract IEnumerator FadeIn(JukeboxSongMetadata metadata);
    protected abstract IEnumerator Await();
    protected abstract IEnumerator FadeOut();

    private IEnumerator ShowPanelRoutine(JukeboxSongMetadata metadata)
    {
        var artist = !string.IsNullOrEmpty(metadata.Artist)
            ? $"<color=#999>{metadata.Artist}</color>"
            : string.Empty;

        text.text = $"{metadata.Title.ToUpper()} {artist}";
        icon.sprite = metadata.Icon != null ? metadata.Icon : defaultIcon;
        yield return FadeIn(metadata);
        yield return Await();
        yield return FadeOut();
    }

    private void NextAudioClip(ClipChangedArgs clip)
    {
        if (!clip.NewSong)
            return;

        active = PrefsManager.Instance.GetIntLocal("jukebox.songPanelStyle") == (int)Style;
        if (displayRoutine != null)
            StopCoroutine(displayRoutine);
        displayRoutine = StartCoroutine(ShowPanelRoutine(clip.Song.Metadata));
    }

    private void OnStop()
    {
        active = false;
    }

    private void OnPrefChanged(string key, object value)
    {
        if (key.Equals("jukebox.showTrackPanelIndefinitely"))
            showIndefinitely = (bool)value;

        if (key.Equals("jukebox.songPanelStyle"))
            active = (int)value == (int)Style;
    }
}