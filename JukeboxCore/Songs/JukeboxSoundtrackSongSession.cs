using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JukeboxCore.Components;
using JukeboxCore.Songs.Models;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace JukeboxCore.Songs;

public sealed class JukeboxSoundtrackSongSession(JukeboxSong song) : JukeboxSongSession(song)
{
    private readonly List<AsyncOperationHandle<AudioClip>> calmClipsHandles = [];
    private AsyncOperationHandle<SoundtrackSong> handle;

    protected override IEnumerator LoadInternal(IEnumerator callback)
    {
        handle = Addressables.LoadAssetAsync<SoundtrackSong>(Song.Id.path);
        yield return handle;
        var addressable = handle.Result;
        IntroClip = addressable.introClip;
        Clips = addressable.clips;

        var calmClips = JukeboxStaticData.Instance?.calmThemes.FindCalmClipsFor(Song.Id.path);
        if (calmClips != null)
        {
            CalmClips = [];
            foreach (var calmClipHandle in calmClips
                         .Select(static clip => Addressables.LoadAssetAsync<AudioClip>(clip.AssetGUID)))
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
