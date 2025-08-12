using System;
using System.IO;
using TagLib;
using UnityEngine;
using File = TagLib.File;
using static JukeboxCore.Utils.CompositeSongsUtils;
using static JukeboxCore.Utils.GenericUtils;

namespace JukeboxCore.Models.Song
{
    public class JukeboxSongMetadata
    {
        public Sprite Icon { get; set; }
        public string Title { get; }
        public string Artist { get; }
        public CompositeProperties Composite { get; }

        private JukeboxSongMetadata(Sprite icon, string title, CompositeProperties composite, string artist = default)
        {
            Icon = icon;
            Title = title;
            Composite = composite;
            Artist = artist;
        }

        public static JukeboxSongMetadata From(FileInfo srcFile)
        {
            var composite = GetSongComponents(srcFile);
            var file = composite.GotIntroAndLoop ? WithPostfix(srcFile, "intro") : srcFile;
            try
            {
                var tlFile = File.Create(file.FullName);
                var tag = tlFile.Tag;
                var logo = LoadCover(tag);
                return new JukeboxSongMetadata(
                    logo,
                    !string.IsNullOrEmpty(tag.Title) ? tag.Title : Path.GetFileNameWithoutExtension(srcFile.Name),
                    composite,
                    FirstNonEmpty(tag.FirstPerformer, tag.FirstComposer, tag.FirstAlbumArtist)
                );
            }
            catch (Exception)
            {
                return new JukeboxSongMetadata(default, file.Name, composite);
            }
        }

        public static JukeboxSongMetadata From(AssetReferenceSoundtrackSong reference)
        {
            var asyncOperationHandle = reference.LoadAssetAsync();
            asyncOperationHandle.WaitForCompletion();
            var addressable = asyncOperationHandle.Result;
            return new JukeboxSongMetadata(
                addressable.icon,
                addressable.songName,
                GetSongComponents(reference.AssetGUID, addressable),
                addressable.extraLevelBit);
        }


        private static Sprite LoadCover(Tag tag)
        {
            if (tag.Pictures.Length <= 0)
                return default;
            
            var pic = tag.Pictures[0];
            using var ms = new MemoryStream(pic.Data.Data);
            try
            {
                ms.Seek(0, SeekOrigin.Begin);

                var tex = new Texture2D(2, 2);
                tex.LoadImage(ms.ToArray());

                return Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f), 100.0f);
            }
            catch (Exception)
            {
                return default;
            }
        }

        public class CompositeProperties
        {
            public bool GotIntroAndLoop { get; private set; }
            public bool GotCalmIntro { get; private set; }
            public bool GotCalmTheme { get; private set; }
            public bool GotCalmLoop { get; private set; }

            private CompositeProperties()
            {
            }

            public class Builder
            {
                public Builder()
                {
                    Build = new CompositeProperties();
                }

                public Builder GotIntroAndLoop(bool val)
                {
                    Build.GotIntroAndLoop = val;
                    return this;
                }
                
                public Builder GotCalmIntro(bool val)
                {
                    Build.GotCalmIntro = val;
                    return this;
                }
                
                public Builder GotCalmTheme(bool val)
                {
                    Build.GotCalmTheme = val;
                    return this;
                }
                
                public Builder GotCalmLoop(bool val)
                {
                    Build.GotCalmLoop = val;
                    return this;
                }

                public CompositeProperties Build { get; }
            }
        }
    }
}