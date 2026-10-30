using System;
using System.Collections;
using System.Collections.Generic;
using JukeboxCore.Songs.Models;
using JukeboxCore.Subtitles;
using UnityEngine;

namespace JukeboxCore.Songs;

public abstract class JukeboxSongSession(JukeboxSong song) : IDisposable
{
    public JukeboxSong Song { get; } = song;
    public AudioClip IntroClip { get; protected set; }
    public AudioClip CalmIntroClip { get; protected set; }
    public List<AudioClip> Clips { get; protected set; }
    public List<AudioClip> CalmClips { get; protected set; }
    public int MaxClipsIfNotRepeating { get; protected set; }
    public SubtitleTrack Subtitles { get; } =
        song.Metadata is JukeboxCustomSongMetadata customMetadata ? customMetadata.MainSubtitles : SubtitleTrack.Empty;
    public SubtitleTrack IntroSubtitles { get; } =
        song.Metadata is JukeboxCustomSongMetadata introMetadata ? introMetadata.IntroSubtitles : SubtitleTrack.Empty;
    
    private bool disposed;

    public void Dispose()
    {
        if (disposed)
            return;

        disposed = true;
        DisposeInternal();
        IntroClip = null;
        CalmIntroClip = null;
        Clips = null;
        CalmClips = null;
        MaxClipsIfNotRepeating = -1;
    }

    public IEnumerator Load(IEnumerator callback)
    {
        try
        {
            yield return LoadInternal(callback);
        }
        finally
        {
            Dispose();
        }
    }

    protected abstract IEnumerator LoadInternal(IEnumerator callback);

    protected abstract void DisposeInternal();
}
