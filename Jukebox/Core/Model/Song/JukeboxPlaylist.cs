using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Jukebox.Core.Model.Song
{
    public class JukeboxPlaylist: Playlist
    {
        public JukeboxPlaylist() {}

        public JukeboxPlaylist(IEnumerable<SongIdentifier> passedIds)
            : base(PrefsManager.Instance.GetBoolLocal("jukebox.preventDuplicateTracks") ? passedIds.Distinct() : passedIds)
        {
        }

        public new static string currentPath => Path.Combine(directory.FullName,
            $"{PrefsManager.Instance.GetStringLocal("jukebox.currentPlaylist")}.json");

        public static string PathForPlaylist(string name) => Path.Combine(directory.FullName, $"{name}.json");

        public new void Add(SongIdentifier id)
        {
            if (!PrefsManager.Instance.GetBoolLocal("jukebox.preventDuplicateTracks") || !ids.Contains(id))
                base.Add(id);
        }

        public void RemoveDuplicates()
        {
            var distinct = ids.Distinct().ToList();
            ids.Clear();
            foreach (var id in distinct)
                Add(id);
        }
    }
}