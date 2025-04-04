using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Playlist;
// ReSharper disable VirtualMemberCallInConstructor

namespace Jukebox.Core.Model.Song
{
    public abstract class JukeboxSong
    {
        public SongIdentifier Id { get; }
        public JukeboxSongMetadata Metadata => lazyMetadata.Value;
        public AudioClip IntroClip { get; protected set; }
        public AudioClip CalmIntroClip { get; protected set; }
        public List<AudioClip> Clips { get; protected set; }
        public List<AudioClip> CalmClips { get; protected set; }
        public int MaxClipsIfNotRepeating { get; protected set; }
        public bool IsCustom => Id.type == SongIdentifier.IdentifierType.File;
        public string ArtistAndTrack => Metadata.Artist != null ? Metadata.Artist + " — " + Metadata.Title : Metadata.Title;
        
        protected abstract Lazy<JukeboxSongMetadata> GetMetadata { get; }
        private readonly Lazy<JukeboxSongMetadata> lazyMetadata;

        protected JukeboxSong(SongIdentifier id)
        {
            Id = id;
            lazyMetadata = GetMetadata;
        }

        public IEnumerator Acquire(IEnumerator callback)
        {
            yield return AcquireInternal(callback);
            Dispose();
        }

        private void Dispose()
        {
            DisposeInternal();
            IntroClip = null;
            CalmIntroClip = null;
            Clips = null;
            CalmClips = null;
            MaxClipsIfNotRepeating = -1;
        }

        protected abstract IEnumerator AcquireInternal(IEnumerator callback);
        
        protected abstract void DisposeInternal();
    }
}