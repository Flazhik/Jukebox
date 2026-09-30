using JukeboxCore.Assets;
using JukeboxCore.Components;
using UnityEngine;

namespace JukeboxCore.Songs.Models;

public sealed class JukeboxSoundtrackSongMetadata : JukeboxSongMetadata
{
    private readonly string guid;
    private readonly SoundtrackSong song;

    private JukeboxSoundtrackSongMetadata(Sprite icon, string title, string guid, SoundtrackSong song, string artist)
        : base(icon, title, artist)
    {
        this.guid = guid;
        this.song = song;
    }

    public static JukeboxSoundtrackSongMetadata From(AssetReferenceSoundtrackSong reference)
    {
        var addressable = AssetsManager.Load<SoundtrackSong>(reference.AssetGUID);
        return new JukeboxSoundtrackSongMetadata(addressable.icon, addressable.songName,
            reference.AssetGUID, addressable, addressable.extraLevelBit);
    }

    public override CompositeProperties GetCompositeProperties()
    {
        return new CompositeProperties
        {
            GotIntroAndLoop = song.introClip != null,
            GotCalmTheme = JukeboxStaticData.Instance?.calmThemes.FindCalmClipsFor(guid) != null
        };
    }
}
