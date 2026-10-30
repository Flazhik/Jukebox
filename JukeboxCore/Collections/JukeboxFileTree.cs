using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JukeboxCore.Components;
using JukeboxCore.Songs.Models;
using static Playlist;
using static Playlist.SongIdentifier;
using static JukeboxCore.Utils.CompositeSongsUtils;

namespace JukeboxCore.Collections;

public class JukeboxFileTree : IDirectoryTree<JukeboxSong>
{
    public JukeboxFileTree(string path, IDirectoryTree<JukeboxSong> parent = null)
    {
        RealDirectory = new DirectoryInfo(path);
        this.parent = parent;
        Refresh();
    }

    private JukeboxFileTree(DirectoryInfo realDirectory, IDirectoryTree<JukeboxSong> parent = null)
    {
        RealDirectory = realDirectory;
        this.parent = parent;
        Refresh();
    }

    private DirectoryInfo RealDirectory { get; }
    public string name { get; private set; }
    public IDirectoryTree<JukeboxSong> parent { get; set; }
    public IEnumerable<IDirectoryTree<JukeboxSong>> children { get; private set; }
    public IEnumerable<JukeboxSong> files { get; private set; }

    public void Refresh()
    {
        RealDirectory.Create();
        name = RealDirectory.Name;
        var directories = GetYouTubeFolderOnTop(RealDirectory.GetDirectories());
        children = directories.Select(dir => new JukeboxFileTree(dir, this));

        files = RealDirectory
            .GetFiles()
            .Where(HasValidExtenstion)
            .GroupBy(static file => WithoutPostfix(file).FullName)
            .Select(group => BuildSong(group.ToList()))
            .ToList();
        return;

        JukeboxSong BuildSong(List<FileInfo> group)
        {
            var baseFile = WithoutPostfix(group[0]);
            return GetTrackSegments(baseFile, group).IsPlayable
                ? JukeboxSongsLoader.Instance?.Get(new SongIdentifier(baseFile.FullName, IdentifierType.File))
                : null;
        }
    }

    public IEnumerable<JukeboxSong> GetFilesRecursive()
    {
        return children.SelectMany(child => child.GetFilesRecursive()).Concat(files);
    }

    public override bool Equals(object obj)
    {
        return obj != null
               && !(GetType() != obj.GetType())
               && string.Equals(RealDirectory.FullName, (obj as DirectoryInfo)?.FullName,
                   StringComparison.InvariantCultureIgnoreCase);
    }

    public override int GetHashCode()
    {
        return RealDirectory.GetHashCode();
    }

    private DirectoryInfo[] GetYouTubeFolderOnTop(DirectoryInfo[] directories)
    {
        if (parent == null
            && directories.Length > 1
            && directories.FirstOrDefault(static folder => folder.Name.Equals("YouTube")) is { } ytFolder)
            return directories.Where(e => !e.Equals(ytFolder)).Prepend(ytFolder).ToArray();

        return directories;
    }
}