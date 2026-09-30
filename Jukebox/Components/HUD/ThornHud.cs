using System;
using JukeboxCore.Events;
using JukeboxCore.Playback;
using UnityEngine;

namespace Jukebox.Components.HUD;

public abstract class ThornHud : MonoBehaviour
{
    protected abstract void Refresh();

    [SerializeField] protected GameObject hud;
    [NonSerialized] protected IPlaybackController player;

    private void Awake()
    {
        RestoreFontMaterials();
    }

    protected virtual void OnEnable()
    {
        player = JukeboxManager.Instance?.player;
        if (player == null)
        {
            enabled = false;
            return;
        }
        
        if (player.State?.Song != null)
            hud.SetActive(true);

        Refresh();
        player.OnNextAudioClip += NextAudioClip;
        player.OnStop += OnStop;
    }

    protected virtual void OnDisable()
    {
        if (player == null)
            return;

        player.OnNextAudioClip -= NextAudioClip;
        player.OnStop -= OnStop;
    }

    public void RestoreFontMaterials() => HudFontMaterials.RestoreOverlay(transform);

    private void NextAudioClip(ClipChangedArgs clip)
    {
        hud.SetActive(true);
        Refresh();
    }

    private void OnStop()
    {
        hud.SetActive(false);
    }
}
