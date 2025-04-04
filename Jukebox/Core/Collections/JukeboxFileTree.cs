using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jukebox.Components;
using Jukebox.Core.Model.Song;
using static Playlist.SongIdentifier;
using static Jukebox.Utils.CompositeSongsUtils;
using static Playlist;
namespace Jukebox.Core.Collections
{
    public class JukeboxFileTree : IDirectoryTree<JukeboxSong>
    {
        public string name { get; private set; }
        public IDirectoryTree<JukeboxSong> parent { get; set; }
        public IEnumerable<IDirectoryTree<JukeboxSong>> children { get; private set; }
        public IEnumerable<JukeboxSong> files { get; private set; }
        private DirectoryInfo RealDirectory { get; }

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

        public void Refresh()
        {
            RealDirectory.Create();
            name = RealDirectory.Name;
            children = RealDirectory.GetDirectories().Select(dir => new JukeboxFileTree(dir, this));

            files = RealDirectory
                .GetFiles()
                .Where(HasValidExtenstion)
                .GroupBy(file => WithoutPostfix(file).FullName)
                .Select(group => JukeboxSongsLoader.Instance.Load(new SongIdentifier(group.Key, IdentifierType.File)))
                .ToList();
        }

        public override bool Equals(object obj) =>
            obj != null
            && !(GetType() != obj.GetType())
            && string.Equals(RealDirectory.FullName, (obj as DirectoryInfo)?.FullName, StringComparison.InvariantCultureIgnoreCase);

        public override int GetHashCode() => RealDirectory.GetHashCode();

        public IEnumerable<JukeboxSong> GetFilesRecursive() =>
            children.SelectMany(child => child.GetFilesRecursive()).Concat(files);
    }
}