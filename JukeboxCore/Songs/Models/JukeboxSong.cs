using System;
using System.IO;
using static Playlist;

namespace JukeboxCore.Songs.Models;

public sealed class JukeboxSong
{
    private readonly Lazy<JukeboxSongMetadata> metadata;

    private JukeboxSong(SongIdentifier id, Func<JukeboxSongMetadata> metadataFactory)
    {
        Id = id;
        metadata = new Lazy<JukeboxSongMetadata>(metadataFactory);
    }

    public SongIdentifier Id { get; }
    public JukeboxSongMetadata Metadata => metadata.Value;
    public bool IsCustom => Id.type == SongIdentifier.IdentifierType.File;
    public string ArtistAndTrack => Metadata.Artist != null ? Metadata.Artist + " — " + Metadata.Title : Metadata.Title;

    public static JukeboxSong Create(SongIdentifier id)
    {
        return id.type switch
        {
            SongIdentifier.IdentifierType.File => new JukeboxSong(id,
                () => JukeboxCustomSongMetadata.From(new FileInfo(id.path))),
            SongIdentifier.IdentifierType.Addressable => new JukeboxSong(id,
                () => JukeboxSoundtrackSongMetadata.From(new AssetReferenceSoundtrackSong(id.path))),
            _ => throw new ArgumentOutOfRangeException(nameof(id))
        };
    }
}
