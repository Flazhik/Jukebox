using System;
using Jukebox.Utils;
using TMPro;
using UnityEngine;

namespace Jukebox.Components.HUD;

public class PlaybackTimeThornHud : ThornHud
{
    [SerializeField] private TMP_Text timestamp;
    [SerializeField] private TMP_Text totalDuration;
    [SerializeField] private TMP_Text currentClip;
    [SerializeField] private GameObject bar;

    [NonSerialized] private float? progressBarWidth;
    [NonSerialized] private float clipDuration;

    private void Update()
    {
        if (player.Source == null || player.Source.clip == null || clipDuration == 0)
            return;

        var elapsed = player.Source.time;
        timestamp.text = elapsed.SecondsToHumanReadable();
        var rect = bar.GetComponent<RectTransform>();
        if (progressBarWidth.HasValue)
            rect.sizeDelta = new Vector2(progressBarWidth.Value * (-1 + elapsed / clipDuration), rect.sizeDelta.y);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        progressBarWidth ??= ((RectTransform)bar.transform).rect.width;
    }

    protected override void Refresh()
    {
        if (player.Source?.clip == null || player.State == null)
            return;

        clipDuration = player.Source.clip.length;
        totalDuration.text = clipDuration.SecondsToHumanReadable();
        currentClip.text = player.State.ClipIndex switch
        {
            -1 => "",
            0 => "INTRO",
            _ => player.State.ClipCount > 1 ? $"{player.State.ClipIndex}/{player.State.ClipCount}" : ""
        };
    }
}
