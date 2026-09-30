using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using JukeboxCore.Songs.Models;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace JukeboxCore.Songs;

public sealed class JukeboxCustomSongSession(JukeboxSong song, CompositeSongFiles files) : JukeboxSongSession(song)
{
    private readonly List<UnityWebRequest> requests = [];
    private readonly List<AudioClip> retired = [];
    private List<DownloadHandlerAudioClip> handles;
    private int fullyLoadedIndex = -1;

    ~JukeboxCustomSongSession()
    {
        requests.ForEach(static request => request.Dispose());
        requests.Clear();
    }
    
    /// <summary>
    /// Replaces streaming clips with the cached ones
    /// </summary>
    /// <param name="index">Clip index</param>
    /// <param name="onLoaded">Callback</param>
    /// <returns></returns>
    public IEnumerator LoadFullyFor(int index, Action<AudioClip, AudioClip> onLoaded)
    {
        var battleFile = index == 0 ? files.Intro : files.Main;
        var calmFile = index == 0 ? files.CalmIntro : files.Calm;

        AudioClip battleClip = null;
        AudioClip calmClip = null;

        if (battleFile != null)
            yield return Download(battleFile, false, clip => battleClip = clip);

        if (calmFile != null)
            yield return Download(calmFile, false, clip => calmClip = clip);

        ReplaceClips(index, battleClip, calmClip);
        fullyLoadedIndex = index;
        onLoaded(battleClip, calmClip);
    }

    public bool IsFullyLoaded(int index) => fullyLoadedIndex == index;

    protected override IEnumerator LoadInternal(IEnumerator callback)
    {
        handles = [];

        if (files.HasIntro)
            yield return Download(files.Intro, true, c => IntroClip = c);

        yield return Download(files.Main, true, c => Clips = [c]);

        if (files.CalmIntro != null)
            yield return Download(files.CalmIntro, true, c => CalmIntroClip = c);

        if (files.Calm != null)
            yield return Download(files.Calm, true, c => CalmClips = [c]);

        yield return callback;
    }

    /// <summary>
    /// Returns an AudioClip for path
    /// </summary>
    /// <param name="path">A path to the track</param>
    /// <param name="stream">Defines whether to stream the audio or cache it</param>
    /// <param name="downloadCallback">Callback</param>
    /// <returns></returns>
    private IEnumerator Download(FileSystemInfo path, bool stream, Action<AudioClip> downloadCallback)
    {
        var request = UnityWebRequestMultimedia.GetAudioClip(new Uri(path.FullName),
            CustomMusicFileBrowser.extensionTypeDict[path.Extension.ToLower()]);
        requests.Add(request);

        if (request.downloadHandler is not DownloadHandlerAudioClip handle)
            yield break;

        if (stream)
            handles.Add(handle);

        handle.streamAudio = stream;
        request.SendWebRequest();
        yield return request;
        downloadCallback(handle.audioClip);
    }

    protected override void DisposeInternal()
    {
        DisposeOfRequests();
        DisposeOfHandlers();
        handles = null;

        DestroyClipIfPresent(IntroClip);
        DestroyClipIfPresent(CalmIntroClip);
        DestroyClipsIfPresent(Clips);
        DestroyClipsIfPresent(CalmClips);

        foreach (var clip in retired)
            DestroyClipIfPresent(clip);

        retired.Clear();
    }
    
    private void ReplaceClips(int clipIndex, AudioClip battleClip, AudioClip calmClip)
    {
        if (clipIndex == 0)
        {
            Retire(IntroClip);
            Retire(CalmIntroClip);
            IntroClip = battleClip;
            CalmIntroClip = calmClip;
            return;
        }

        RetireAll(Clips);
        RetireAll(CalmClips);
        Clips = battleClip == null ? null : [battleClip];
        CalmClips = calmClip == null ? null : [calmClip];
    }

    private void Retire(AudioClip clip)
    {
        if (clip != null)
            retired.Add(clip);
    }

    private void RetireAll(List<AudioClip> clips)
    {
        if (clips == null)
            return;

        foreach (var clip in clips)
            Retire(clip);
    }

    private void DisposeOfRequests()
    {
        requests.ForEach(static request => request.Dispose());
        requests.Clear();
    }

    private void DisposeOfHandlers()
    {
        handles.ForEach(static request => request.Dispose());
    }

    private static void DestroyClipsIfPresent(List<AudioClip> clips)
    {
        if (clips == null)
            return;

        foreach (var clip in clips)
            DestroyClipIfPresent(clip);
    }

    private static void DestroyClipIfPresent(AudioClip clip)
    {
        if (clip != null)
            Object.Destroy(clip);
    }
}
