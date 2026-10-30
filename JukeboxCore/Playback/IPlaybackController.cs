using System;
using System.Collections.Generic;
using JukeboxCore.Events;
using UnityEngine;

namespace JukeboxCore.Playback;

public interface IPlaybackController
{
    PlaybackState State { get; }
    AudioSource Source { get; }

    event Action<ClipChangedArgs> OnNextAudioClip;
    event Action<IEnumerable<Playlist.SongIdentifier>> OnOrderChange;
    event Action OnStop;
    event Action<string> OnSubtitleChanged;

    void ChangeTrack(int index);
    void RewindTo(float position);
    void Stop();
}