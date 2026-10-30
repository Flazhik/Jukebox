using System;
using System.IO;
using JukeboxCore.Logging;
using JukeboxCore.Subtitles;
using JukeboxCore.Utils;
using TagLib;
using UnityEngine;
using UnityEngine.AddressableAssets;
using File = TagLib.File;
using static JukeboxCore.Utils.CompositeSongsUtils;
using static JukeboxCore.Utils.GenericUtils;

namespace JukeboxCore.Songs.Models;

public sealed class JukeboxCustomSongMetadata : JukeboxSongMetadata
{
    private static Sprite defaultIcon;

    private static Sprite DefaultIcon => defaultIcon;

    private JukeboxCustomSongMetadata(Sprite icon, string title, CompositeSongFiles files, string artist = null)
        : base(icon, title, artist)
    {
        Files = files;
    }

    public CompositeSongFiles Files { get; }

    public SubtitleTrack MainSubtitles => field ??= SubtitleParser.FromFile(SubtitleFiles.MainSidecar(Files.Main));

    public SubtitleTrack IntroSubtitles => field ??= SubtitleParser.FromFile(SubtitleFiles.IntroSidecar(Files.Intro));
    
    public override CompositeProperties GetCompositeProperties()
    {
        return new CompositeProperties
        {
            GotIntroAndLoop = Files.HasIntro,
            GotCalmIntro = Files.CalmIntro != null,
            GotCalmTheme = Files.Calm != null
        };
    }

    public static void Load() =>
        defaultIcon = Addressables.LoadAssetAsync<Sprite>("Assets/Jukebox/Textures/music_note.png").WaitForCompletion();

    public static JukeboxCustomSongMetadata From(FileInfo src)
    {
        try
        {
            var files = GetTrackSegments(src);
            var defaultTitle = PathsUtils.GetFileNameWithoutExtension(src.Name);

            if (!files.IsPlayable)
                return new JukeboxCustomSongMetadata(DefaultIcon, defaultTitle, files);

            var tagsFile = File.Create(files.Main.FullName);
            var tag = tagsFile.Tag;
            return new JukeboxCustomSongMetadata(
                LoadCover(tag) ?? DefaultIcon,
                !string.IsNullOrEmpty(tag.Title) ? tag.Title : defaultTitle,
                files,
                FirstNonEmpty(tag.FirstPerformer, tag.FirstComposer, tag.FirstAlbumArtist)
            );
        }
        catch (Exception e)
        {
            JukeboxLogger.Log.LogError($"Couldn't retrieve metadata for {src.Name}\n{e}");
            return null;
        }
    }

    private static Sprite LoadCover(Tag tag)
    {
        if (tag.Pictures.Length <= 0)
            return null;

        var pic = tag.Pictures[0];
        using var ms = new MemoryStream(pic.Data.Data);
        try
        {
            ms.Seek(0, SeekOrigin.Begin);
            var tex = new Texture2D(2, 2);
            tex.LoadImage(ms.ToArray());
            return Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
