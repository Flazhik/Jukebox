using System.Collections.Generic;
using System.IO;
using System.Linq;
using JukeboxCore.Utils;
using static Playlist.SongIdentifier;

namespace JukeboxCore.Songs.Models;

public class JukeboxPlaylist : Playlist
{
    public JukeboxPlaylist()
    {
    }

    public JukeboxPlaylist(IEnumerable<SongIdentifier> passedIds)
        : base(PrefsManager.Instance != null
               && PrefsManager.Instance.GetBoolLocal("jukebox.preventDuplicateTracks")
            ? passedIds.Distinct()
            : passedIds)
    {
    }

    public static string CurrentPath => Path.Combine(directory.FullName,
        $"{PrefsManager.Instance?.GetStringLocal("jukebox.currentPlaylist")}.json");

    public static string PathForPlaylist(string name)
    {
        return Path.Combine(directory.FullName, $"{name}.json");
    }

    public new void Add(SongIdentifier id)
    {
        if (!PrefsManager.Instance!.GetBoolLocal("jukebox.preventDuplicateTracks") || !ids.Contains(id))
            base.Add(id);
    }

    public void RemoveDuplicates()
    {
        var distinct = ids.Distinct().ToList();
        ids.Clear();
        foreach (var id in distinct)
            Add(id);
    }

    public void RemoveUnplayable()
    {
        var existing = ids
            .Where(static id => id.type == IdentifierType.Addressable
                                || CompositeSongsUtils.IsPlayableTrack(new FileInfo(id.path)))
            .ToList();

        ids.Clear();
        foreach (var id in existing)
            Add(id);
    }
}