using JukeboxCore.Components;
using UnityEngine;
using UnityEngine.AddressableAssets;

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
        var addressable = Addressables.LoadAssetAsync<SoundtrackSong>(reference.AssetGUID).WaitForCompletion();
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
