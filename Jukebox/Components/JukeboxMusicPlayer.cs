using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Jukebox.Core.Model.Song;
using Jukebox.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using static Playlist.LoopMode;
using static Playlist;
namespace Jukebox.Components
{
    public class JukeboxMusicPlayer : MonoBehaviour
    {
        private static bool AlwaysPlayIntro => PrefsManager.Instance.GetBoolLocal("jukebox.alwaysPlayIntro");
        
        [SerializeField]
        private JukeboxPlaylistEditor playlistEditor;
        
        [SerializeField]
        private JukeboxSongsLoader loader;

        [SerializeField]
        private JukeboxMusicChanger changer;
        
        public static Action<JukeboxSong> onNextSong;
        public static Action<IEnumerable<SongIdentifier>> onOrderChange;
        public static Action onStop;

        public AudioSource Source => changer.battle;
        public static JukeboxSong CurrentSong { get; private set; }
        public static int CurrentSongIndex { get; private set; }
        public float VolumeBoost { get; private set; }

        private Coroutine playlistRoutine;
        private bool forcedChange;
        private bool stopped;
        private bool allowVolumeBoost;

        public void Awake()
        {
            changer = gameObject.GetComponent<JukeboxMusicChanger>();
            onNextSong += OnNextSong;
            PrefsManager.onPrefChanged += OnPrefChanged;
            JukeboxInputs.Instance.DisablePlayer.performed += OnPlayerDisabled;
        }

        public void OnDestroy()
        {
            CurrentSong = null;
            onNextSong -= OnNextSong;
            PrefsManager.onPrefChanged -= OnPrefChanged;
            JukeboxInputs.Instance.DisablePlayer.performed -= OnPlayerDisabled;
        }

        public void OnEnable() => StartPlaylist();

        public void StartPlaylist()
        {
            stopped = false;
            if (playlistEditor.playlist.Count < 1)
                Debug.LogError("No songs in playlist, somehow. Not starting playlist routine...");
            else
                playlistRoutine = StartCoroutine(PlaylistRoutine());
        }
        
        public void Stop()
        {
            stopped = true;
        }

        public void StopPlaylist()
        {
            Source.Stop();
            StopCoroutine(playlistRoutine);
            CurrentSong = null;
            stopped = true;
        }

        public void ChangeTrack(int index)
        {
            CurrentSongIndex = index;
            forcedChange = true;
        }

        private IEnumerator PlaylistRoutine()
        {
            var songFinished = new WaitUntil(() => 
                ((!Source.isPlaying && Application.isFocused)
                || JukeboxInputs.Instance.NextTrack.WasPerformedThisFrame()
                || forcedChange)
                && !stopped);

            var first = true;
            var playlist = playlistEditor.playlist;
            var shuffled = playlist.shuffled
                ? new DeckShuffled<SongIdentifier>(playlist.ids).AsEnumerable()
                : playlist.ids.AsEnumerable();
            
            while (!stopped)
            {
                if (shuffled is DeckShuffled<SongIdentifier> deckShuffled)
                    deckShuffled.Reshuffle();

                var currentOrder = shuffled.ToList();
                onOrderChange.Invoke(currentOrder);
                CurrentSongIndex = playlist.loopMode == LoopOne
                    ? currentOrder.FindIndex(id => Equals(id, playlist.ids[playlist.selected]))
                    : 0;

                for (; CurrentSongIndex < currentOrder.Count; CurrentSongIndex++)
                {
                    forcedChange = false;
                    var id = currentOrder[CurrentSongIndex];
                    var song = loader.Load(id);
                    onNextSong?.Invoke(song);
                    PresenceController.UpdateCyberGrindWave(EndlessGrid.Instance.currentWave);
                    yield return song.Acquire(Play(first));
                    first = false;

                    IEnumerator Play(bool firstSong)
                    {
                        if ((firstSong || AlwaysPlayIntro) && song.IntroClip != null)
                        {
                            changer.ChangeTo(song.IntroClip, song.CalmIntroClip);
                            yield return songFinished;
                        }

                        var clipsPlayed = 0;

                        for (var i = 0; i < song.Clips.Count; i++)
                        {
                            if (stopped)
                                break;
                            do
                            {
                                changer.ChangeTo(song.Clips[i], song.CalmClips?.Count > i ? song.CalmClips[i] : null);
                                yield return songFinished;
                                ++clipsPlayed;
                            } while (playlist.loopMode == LoopOne
                                     && !stopped
                                     && !forcedChange
                                     && !JukeboxInputs.Instance.NextTrack.WasPerformedThisFrame());

                            if (playlist.loopMode != LoopOne && song.MaxClipsIfNotRepeating > 0 &&
                                clipsPlayed >= song.MaxClipsIfNotRepeating)
                                break;
                        }
                    }
                }
            }
        }

        private void OnPlayerDisabled(InputAction.CallbackContext _)
        {
            if (!stopped)
            {
                StopPlaylist();
                onStop.Invoke();
            }
            else
                StartPlaylist();
        }

        private void OnPrefChanged(string key, object value)
        {
            if (!key.Equals("jukebox.volumeBoost"))
                return;

            VolumeBoost = allowVolumeBoost ? (float)value : 0;
        }
        
        private void OnNextSong(JukeboxSong song)
        {
            CurrentSong = song;
            allowVolumeBoost = song.IsCustom;
            VolumeBoost = allowVolumeBoost ? PrefsManager.Instance.GetFloatLocal("jukebox.volumeBoost") : 0;
        }
    }
}