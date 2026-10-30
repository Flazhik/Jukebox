using Jukebox.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Jukebox.Components.HUD;

public class NowPlayingThornHud : ThornHud
{
    [SerializeField] private Image cover;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text artist;

    protected override void Refresh()
    {
        var song = player.State?.Song;
        if (song == null)
            return;

        cover.sprite = song.Metadata.Icon;
        title.SetPreservingRtl(song.Metadata.Title);
        artist.SetPreservingRtl(song.Metadata.Artist);
    }
}
