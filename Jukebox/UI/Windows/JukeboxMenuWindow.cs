using Jukebox.Components;
using JukeboxCore.Events;
using JukeboxCore.Playback;
using SettingsMenu.Components;
using UnityEngine;
using UnityEngine.UI;
using static JukeboxCore.Utils.ReflectionUtils;
using Settings = SettingsMenu.Components.SettingsMenu;

namespace Jukebox.UI.Windows;

public class JukeboxMenuWindow : JukeboxWindow
{
    [SerializeField] protected Button[] buttons;
    [SerializeField] protected GameObject activeBgObject;
    [SerializeField] protected GameObject generalSettings;
    [SerializeField] protected Button downloaderButton;
    [SerializeField] protected GameObject downloaderPage;

    private IPlaybackController player;
    
    public override string StateKey() => "jukeboxMenu";

    protected override void Awake()
    {
        base.Awake();
        foreach (var button in buttons)
            button.onClick.AddListener(() => SetActiveButton(button));

        player = JukeboxManager.Instance?.player;
        player?.OnNextAudioClip += NextAudioClip;
    }

    protected void OnDestroy()
    {
        player.OnNextAudioClip -= NextAudioClip;
    }

    public void NextAudioClip(ClipChangedArgs clip)
    {
        if (!clip.NewSong || !clip.Song.IsCustom)
            return;

        var settingsSlider = generalSettings.GetComponentInChildren<SettingsSlider>(true);
        var slider = GetPrivate<Slider>(settingsSlider, typeof(SettingsSlider), "slider");
        slider.value = player.State.VolumeBoost;
    }

    public void OpenDownloader()
    {
        var settings = gameObject.GetComponent<Settings>();
        if (!GetPrivate<bool>(settings, typeof(Settings), "initialized"))
            settings.Initialize();
        settings.SetActivePage(downloaderPage);
        SetActiveButton(downloaderButton);
    }

    private void SetActiveButton(Button button)
    {
        activeBgObject.transform.SetParent(button.transform);
        activeBgObject.transform.SetSiblingIndex(0);
        activeBgObject.transform.localPosition = Vector3.zero;
    }
}