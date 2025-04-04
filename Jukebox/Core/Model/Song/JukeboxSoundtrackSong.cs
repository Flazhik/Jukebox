using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Jukebox.Components;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Jukebox.Core.Model.Song
{
    public sealed class JukeboxSoundtrackSong: JukeboxSong
    {
        private AsyncOperationHandle<SoundtrackSong> handle;
        private readonly List<AsyncOperationHandle<AudioClip>> calmClipsHandles = new ();
        
        private JukeboxSoundtrackSong(string path)
            : base(new Playlist.SongIdentifier(path, Playlist.SongIdentifier.IdentifierType.Addressable))
        {
        }

        public static JukeboxSoundtrackSong Create(string path) => new(path);

        protected override Lazy<JukeboxSongMetadata> GetMetadata => new(() =>
            JukeboxSongMetadata.From(new AssetReferenceSoundtrackSong(Id.path)));

        protected override IEnumerator AcquireInternal(IEnumerator callback)
        {
            handle = Addressables.LoadAssetAsync<SoundtrackSong>(Id.path);
            yield return handle;
            var addressable = handle.Result;
            IntroClip = addressable.introClip;
            Clips = addressable.clips;

            var calmClips = JukeboxManager.Instance.calmThemes.FindCalmClipsFor(Id.path);

            if (calmClips != default)
            {
                CalmClips = new List<AudioClip>();
                foreach (var calmClipHandle in calmClips
                             .Select(clip => Addressables.LoadAssetAsync<AudioClip>(clip.AssetGUID)))
                {
                    calmClipsHandles.Add(calmClipHandle);
                    yield return calmClipHandle;
                    CalmClips.Add(calmClipHandle.Result);
                }
            }
            
            MaxClipsIfNotRepeating = addressable.maxClipsIfNotRepeating;
            yield return callback;
        }

        protected override void DisposeInternal()
        {
            Addressables.Release(handle);
            foreach (var calmClipHandle in calmClipsHandles)
                Addressables.Release(calmClipHandle);
            
            calmClipsHandles.Clear();
            handle = new AsyncOperationHandle<SoundtrackSong>();
        }
    }
}