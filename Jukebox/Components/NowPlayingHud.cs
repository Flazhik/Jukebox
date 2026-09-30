using System;
using System.Linq;
using Jukebox.Components.HUD;
using Jukebox.Utils;
using JukeboxCore.Events;
using JukeboxCore.Playback;
using SettingsMenu.Components.Pages;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Jukebox.Components;

public class NowPlayingHud : MonoBehaviour
{
    [SerializeField] protected GameObject hud;
    [SerializeField] protected HudType type;
    [SerializeField] protected Image[] background;
    [SerializeField] protected Image cover;
    [SerializeField] protected TMP_Text title;
    [SerializeField] protected TMP_Text artist;
    [SerializeField] protected TMP_Text timestamp;
    [SerializeField] protected TMP_Text totalDuration;
    [SerializeField] protected TMP_Text currentClip;
    [SerializeField] protected GameObject bar;
    
    [Header("Reference materials")]
    [SerializeField] public Material hudMaterial;

    [NonSerialized] private float? progressBarWidth;
    [NonSerialized] private float clipDuration;
    [NonSerialized] private Mask[] masks;
    [NonSerialized] private IPlaybackController player;

    private static int HudTypePref => PrefsManager.Instance!.GetInt("hudType");
    private static bool EnableHud => PrefsManager.Instance!.GetBoolLocal("jukebox.nowPlayingHud");

    protected void Update()
    {
        if (player.Source == null || player.Source.clip == null || clipDuration == 0)
            return;
        var elapsed = player.Source.time;

        if (type == HudType.Standard)
            timestamp.text = elapsed.SecondsToHumanReadable();
        var rect = bar.GetComponent<RectTransform>();
        if (progressBarWidth.HasValue)
            rect.sizeDelta = new Vector2(progressBarWidth.Value * (-1 + elapsed / clipDuration), rect.sizeDelta.y);
    }

    protected void OnEnable()
    {
        player = JukeboxManager.Instance?.player;

        if (player == null)
        {
            enabled = false;
            return;
        }
        masks = GetComponentsInChildren<Mask>(true);
        
        player.OnNextAudioClip += NextAudioClip;
        player.OnStop += OnStop;
        PrefsManager.onPrefChanged += OnPrefChanged;

        progressBarWidth ??= ((RectTransform)bar.transform).rect.width;

        SetOpacity(PrefsManager.Instance!.GetFloat("hudBackgroundOpacity"));
        SetAlwaysOnTop(PrefsManager.Instance!.GetBool("hudAlwaysOnTop"));
        
        hud.transform.localPosition = HudPosition();
    }

    protected void OnDisable()
    {
        if (player == null)
            return;
        
        player.OnNextAudioClip -= NextAudioClip;
        player.OnStop -= OnStop;
        PrefsManager.onPrefChanged -= OnPrefChanged;
    }

    private void NextAudioClip(ClipChangedArgs clip)
    {
        var hudType = HudTypePref;
        hud.SetActive(EnableHud && (type == HudType.Standard ? hudType == 1 : hudType > 1));

        var song = clip.Song;
        cover.sprite = song.Metadata.Icon;
        title.text = song.Metadata.Title;
        artist.text = song.Metadata.Artist;

        clipDuration = player.Source.clip.length;
        if (type != HudType.Standard)
            return;
        
        totalDuration.text = player.Source.clip.length.SecondsToHumanReadable();
        currentClip.text = player.State.ClipIndex switch
        {
            -1 => "",
            0 => "INTRO",
            _ => player.State.ClipCount > 1
                ? $"{player.State.ClipIndex}/{player.State.ClipCount}"
                : ""
        };
    }

    private void OnStop()
    {
        hud.SetActive(false);
    }

    /**
     * Yeah, I know, it's a mess
     */
    private Vector3 HudPosition()
    {
        var hudPos = hud.transform.localPosition;
        if (type == HudType.Standard)
        {
            var y = 30;

            if (HUDSettings.weaponIconEnabled)
                y += 400;

            if (PrefsManager.Instance?.GetInt("speedometer") > 0)
                y += 100;

            return new Vector3(hudPos.x, y, hudPos.z);
        }

        var x = -147;
        if (!PrefsManager.Instance!.GetBool("armIcons"))
            x -= 80;

        return new Vector3(x, hudPos.y, hudPos.z);
    }

    private void SetOpacity(float value)
    {
        foreach (var bg in background)
            bg.color = new Color(bg.color.r, bg.color.g, bg.color.b, value / 100);
    }

    private void SetAlwaysOnTop(bool onTop)
    {
        if (onTop)
            HudFontMaterials.RestoreOverlay(transform);
        else
            HudFontMaterials.RestoreNormal(transform);

        hudMaterial.SetFloat("_ZTest", onTop ? 8f : 4f);

        if (masks == null)
            return;

        foreach (var mask in masks.Where(static mask => mask.enabled))
        {
            mask.enabled = false;
            mask.enabled = true;
        }
    }
    
    private void OnPrefChanged(string key, object value)
    {
        switch (key)
        {
            case "weaponIcons":
            case "speedometer":
            case "armIcons":
                hud.transform.localPosition = HudPosition();
                break;
            case "hudBackgroundOpacity":
                SetOpacity((float)value);
                break;
            case "hudAlwaysOnTop":
                SetAlwaysOnTop((bool)value);
                break;
            case "hudType" when player.State.Song != null:
                hud.SetActive(type == HudType.Standard ? (int)value == 1 : (int)value > 1);
                break;
            case "jukebox.nowPlayingHud" when player.State.Song != null:
            {
                hud.SetActive((bool)value);
                if ((bool)value)
                    NextAudioClip(new ClipChangedArgs(player.State.Song, true));
                break;
            }
        }
    }
    
    protected enum HudType
    {
        Standard,
        Classic
    }
}