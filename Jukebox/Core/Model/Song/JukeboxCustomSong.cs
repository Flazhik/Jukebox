using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Jukebox.Assets;
using UnityEngine;
using UnityEngine.Networking;
using static Jukebox.Utils.CompositeSongsUtils;
using static Playlist;
using Object = UnityEngine.Object;
#pragma warning disable CS0649

namespace Jukebox.Core.Model.Song
{
    public sealed class JukeboxCustomSong: JukeboxSong
    {
        [ExternalAsset("Assets/Jukebox/Textures/music_note.png", typeof(Sprite))]
        private static Sprite defaultIcon;

        private readonly List<UnityWebRequest> requests = new();
        private List<DownloadHandlerAudioClip> handles;

        private JukeboxCustomSong(string path) : base(new SongIdentifier(path, SongIdentifier.IdentifierType.File))
        {
        }

        ~JukeboxCustomSong() => requests.ForEach(request => request.Dispose());

        public static JukeboxCustomSong Create(string path) => new(path);

        protected override Lazy<JukeboxSongMetadata> GetMetadata => new(() =>
        {
            var data = JukeboxSongMetadata.From(new FileInfo(Id.path));
            if (data.Icon == default)
                data.Icon = defaultIcon;
            return data;
        });

        protected override IEnumerator AcquireInternal(IEnumerator callback)
        {
            handles = new List<DownloadHandlerAudioClip>();
            if (Metadata.Composite.GotIntroAndLoop)
                yield return Download(WithPostfix(new FileInfo(Id.path), "intro"), c => IntroClip = c);
            
            yield return Download(Metadata.Composite.GotIntroAndLoop
                ? WithPostfix(new FileInfo(Id.path), "loop")
                : new FileInfo(Id.path), c => Clips = new List<AudioClip> { c });
            
            if (Metadata.Composite.GotCalmIntro)
                yield return Download(WithPostfix(new FileInfo(Id.path), "calmintro"),
                    c => CalmIntroClip = c );

            if (Metadata.Composite.GotCalmTheme)
                yield return Download(WithPostfix(new FileInfo(Id.path), "calm"),
                    c => CalmClips = new List<AudioClip> { c });
            
            if (Metadata.Composite.GotCalmLoop)
                yield return Download(WithPostfix(new FileInfo(Id.path), "calmloop"),
                    c => CalmClips = new List<AudioClip> { c });

            yield return callback;

            IEnumerator Download(FileSystemInfo path, Action<AudioClip> downloadCallback)
            {
                var request = UnityWebRequestMultimedia.GetAudioClip(new Uri(path.FullName).AbsoluteUri,
                CustomMusicFileBrowser.extensionTypeDict[path.Extension.ToLower()]);
                requests.Add(request);
            
                var handle = request.downloadHandler as DownloadHandlerAudioClip;
                handles.Add(handle);
                
                if (handle == null)
                    yield break;
            
                handle.streamAudio = true;
                request.SendWebRequest();
                yield return request;
                downloadCallback.Invoke(handle.audioClip);
            }
        }

        protected override void DisposeInternal()
        {
            DisposeOfRequests();
            DisposeOfHandlers();
            handles = null;
            DestroyClipIfPresent(IntroClip);

            foreach (var clip in Clips)
                DestroyClipIfPresent(clip);
        }

        private void DisposeOfRequests() => requests.ForEach(request => request.Dispose());
        
        private void DisposeOfHandlers() => handles.ForEach(request => request.Dispose());

        private static void DestroyClipIfPresent(AudioClip clip)
        {
            if (clip != null)
                Object.Destroy(clip);
        }
    }
}