using System.Collections.Generic;
using System.Linq;
using JukeboxCore.Songs.Models;
using UnityEngine;
using static Playlist;
using static Playlist.LoopMode;

namespace Jukebox.Components.Player;

public class PlaybackOrder(JukeboxPlaylist playlist)
{
    private readonly DeckShuffled<SongIdentifier> deck = playlist.shuffled
        ? new DeckShuffled<SongIdentifier>(playlist.ids)
        : null;

    public List<SongIdentifier> BuildOrder()
    {
        if (deck == null)
            return playlist.ids.ToList();
        
        deck.Reshuffle();
        return deck.ToList();
    }

    public int StartIndex(List<SongIdentifier> order)
    {
        return playlist.loopMode == LoopOne
            ? order.FindIndex(id => Equals(id, playlist.ids[playlist.selected]))
            : 0;
    }

    public static int RandomIndex(int count) => Random.Range(0, count);
}
