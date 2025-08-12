using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using JukeboxCore.Components;
using static System.IO.Path;
using static JukeboxCore.Models.Song.JukeboxSongMetadata;
namespace JukeboxCore.Utils
{
    public static class CompositeSongsUtils
    {
        private static readonly List<string> SpecialPostfixes = new()
        {
            "intro",
            "loop",
            "calm",
            "calmloop",
            "calmintro"
        };

        public static CompositeProperties GetSongComponents(string guid, SoundtrackSong song) =>
            new CompositeProperties.Builder()
                .GotIntroAndLoop(song.introClip != null)
                .GotCalmIntro(false)
                .GotCalmLoop(JukeboxStaticData.Instance.calmThemes.FindCalmClipsFor(guid) != default)
                .Build;

        public static CompositeProperties GetSongComponents(FileInfo fileInfo)
        {
            var builder = new CompositeProperties.Builder();
            var grouped = fileInfo.Directory?
                .GetFiles()
                .Where(file => WithoutPostfix(file).Name.Equals(fileInfo.Name))
                .Select(file =>
                {
                    if (HasSpecialPostfix(file, "calmintro"))
                        builder.GotCalmIntro(true);

                    if (HasSpecialPostfix(file, "calmloop"))
                        builder.GotCalmLoop(true);
                    
                    if (HasSpecialPostfix(file, "calm"))
                        builder.GotCalmTheme(true);

                    return file;
                }).ToList() ?? new List<FileInfo>();

            if (grouped.Any(file => HasSpecialPostfix(file, "intro"))
                && grouped.Any(file => HasSpecialPostfix(file, "loop")))
                builder.GotIntroAndLoop(true);

            return builder.Build;
        }

        public static FileInfo WithPostfix(FileInfo path, string postfix)
        {
            return new FileInfo(
                @$"{GetDirectoryName(path.FullName)}\{GetFileNameWithoutExtension(path.FullName)}_{postfix}{path.Extension}");
        }

        public static FileInfo WithoutPostfix(FileInfo path)
        {
            return !HasAnySpecialPostfix(path)
                ? path
                : new FileInfo(Regex.Replace(path.FullName, @"_[a-zA-Z]+\.[a-zA-Z0-9]+$", path.Extension));
        }

        public static bool HasValidExtenstion(FileInfo path) =>
            CustomMusicFileBrowser.extensionTypeDict.Keys.Any(extension => path.Extension.ToLower().Equals(extension));

        private static bool HasAnySpecialPostfix(FileInfo path)
            => SpecialPostfixes.Any(postfix => HasSpecialPostfix(path, postfix));
        
        private static bool HasSpecialPostfix(FileInfo path, string postfix) 
            => GetFileNameWithoutExtension(path.Name).EndsWith($"_{postfix}");
    }
}