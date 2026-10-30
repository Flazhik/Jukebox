using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JukeboxCore.Songs.Models;
using static System.IO.Path;

namespace JukeboxCore.Utils;

public static class CompositeSongsUtils
{
    private const string IntroPostfix = "intro";
    private const string LoopPostfix = "loop";
    private const string CalmPostfix = "calm";
    private const string CalmLoopPostfix = "calmloop";
    private const string CalmIntroPostfix = "calmintro";

    private static readonly List<string> SpecialPostfixes =
    [
        CalmIntroPostfix,
        CalmLoopPostfix,
        LoopPostfix,
        CalmPostfix,
        IntroPostfix
    ];

    public static CompositeSongFiles GetTrackSegments(FileInfo baseTrack, IEnumerable<FileInfo> siblings = null)
    {
        var name = GetFileNameWithoutExtension(baseTrack.Name);
        var baseName = GetBaseName(name);
        var isBase = name.Equals(baseName);
        var requestedExists = baseTrack.Exists && HasValidExtenstion(baseTrack);

        if (!isBase)
            return requestedExists
                ? new CompositeSongFiles { Main = baseTrack }
                : CompositeSongFiles.Empty;

        var group = (siblings ?? ListSiblings(baseTrack, baseName)).ToList();
        var main = requestedExists
            ? baseTrack
            : FindComponent(group, baseName, LoopPostfix, baseTrack.Extension);

        if (main == null)
            return CompositeSongFiles.Empty;

        return new CompositeSongFiles
        {
            Main = main,
            Intro = FindComponent(group, baseName, IntroPostfix, baseTrack.Extension),
            CalmIntro = FindComponent(group, baseName, CalmIntroPostfix, baseTrack.Extension),
            Calm = FindComponent(group, baseName, CalmPostfix, baseTrack.Extension)
                ?? FindComponent(group, baseName, CalmLoopPostfix, baseTrack.Extension)
        };
    }
    
    public static bool IsPlayableTrack(FileInfo id) => GetTrackSegments(id).IsPlayable;

    public static FileInfo WithoutPostfix(FileInfo path)
    {
        var name = PathsUtils.GetFileNameWithoutExtension(path.Name);
        var baseName = GetBaseName(name);
        return baseName.Equals(name)
            ? path
            : new FileInfo(Combine(path.DirectoryName ?? string.Empty, baseName + path.Extension));
    }

    public static bool HasValidExtenstion(FileInfo path)
    {
        return CustomMusicFileBrowser.extensionTypeDict.Keys.Any(extension =>
            path.Extension.ToLower().Equals(extension));
    }
    
    public static string GetBaseName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return fileName;

        foreach (var suffix in SpecialPostfixes.Select(static postfix => $"_{postfix}")
                     .Where(suffix => fileName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)))
            return fileName[..^suffix.Length];
        
        return fileName;
    }

    private static List<FileInfo> ListSiblings(FileInfo requested, string baseName)
    {
        var directory = requested.Directory;
        if (directory is not { Exists: true })
            return [];

        return directory.GetFiles()
            .Where(HasValidExtenstion)
            .Where(file => GetBaseName(PathsUtils.GetFileNameWithoutExtension(file.Name)).Equals(baseName))
            .ToList();
    }

    private static FileInfo FindComponent(IReadOnlyList<FileInfo> group, string baseName, string postfix, string preferredExtension)
    {
        var target = $"{baseName}_{postfix}";

        return group
            .Where(file => PathsUtils.GetFileNameWithoutExtension(file.Name).Equals(target))
            .OrderBy(file => file.Extension.Equals(preferredExtension, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .ThenBy(static file => file.Name, StringComparer.Ordinal)
            .FirstOrDefault();
    }
}
