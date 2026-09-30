using UnityEngine;

namespace JukeboxCore.Songs.Models;

public abstract class JukeboxSongMetadata(Sprite icon, string title, string artist = null)
{
    public Sprite Icon { get; } = icon;
    public string Title { get; } = title;
    public string Artist { get; } = artist;

    public abstract CompositeProperties GetCompositeProperties();
}
