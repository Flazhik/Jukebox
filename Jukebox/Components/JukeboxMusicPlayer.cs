using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Jukebox.Input;
using JukeboxCore.Components;
using JukeboxCore.Models.Song;
using UnityEngine;
using UnityEngine.InputSystem;
using static Playlist.LoopMode;
using static Playlist;
namespace Jukebox.Components
{
    public class JukeboxMusicPlayer : MonoBehaviour
    {
        private static float VolumeBoostValue => PrefsManager.Instance.GetFloatLocal("jukebox.volumeBoost");
        private static bool AlwaysPlayIntro => PrefsManager.Instance.GetBoolLocal("jukebox.alwaysPlayIntro");
        private static bool IndividualBoostPerTrack => PrefsManager.Instance.GetBoolLocal("jukebox.individualBoostPerTrack");
        
        [SerializeField]
        private JukeboxPlaylistEditor playlistEditor;
        
        [SerializeField]
        private JukeboxSongsLoader loader;

        [SerializeField]
        private JukeboxMusicChanger changer;
        
        public static event Action<JukeboxSong> OnNextSong;
        public static event Action<IEnumerable<SongIdentifier>> OnOrderChange;
        public static event Action OnStop;

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
            PrefsManager.onPrefChanged += OnPrefChanged;
            JukeboxInputs.Instance.DisablePlayer.performed += OnPlayerDisabled;
        }

        public void OnDestroy()
        {
            CurrentSong = null;
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
                OnOrderChange?.Invoke(currentOrder);
                CurrentSongIndex = playlist.loopMode == LoopOne
                    ? currentOrder.FindIndex(id => Equals(id, playlist.ids[playlist.selected]))
                    : 0;

                for (; CurrentSongIndex < currentOrder.Count; CurrentSongIndex++)
                {
                    forcedChange = false;
                    var id = currentOrder[CurrentSongIndex];
                    var song = loader.Load(id);
                    NextSong(song);
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
                OnStop?.Invoke();
            }
            else
                StartPlaylist();
        }

        private void OnPrefChanged(string key, object value)
        {
            switch (key)
            {
                case "jukebox.volumeBoost":
                {
                    if (IndividualBoostPerTrack && CurrentSong is { IsCustom: true })
                        JukeboxVolumeManager.Instance.SetVolumeBoostFor(CurrentSong.Id.path, (float) value);
                    VolumeBoost = (float)value;
                    break;
                }
                case "jukebox.individualBoostPerTrack":
                    VolumeBoost = CalculateBoostFor(CurrentSong);
                    break;
            }
        }

        private void NextSong(JukeboxSong song)
        {
            CurrentSong = song;
            allowVolumeBoost = song.IsCustom;
            VolumeBoost = CalculateBoostFor(song);
            OnNextSong?.Invoke(song);
        }

        private float CalculateBoostFor(JukeboxSong song)
        {
            if (!allowVolumeBoost)
                return 0;

            if (!IndividualBoostPerTrack)
                return VolumeBoostValue;

            if (song == null || JukeboxVolumeManager.Instance.GetVolumeBoostFor(song.Id.path) is not { } boost)
                return VolumeBoostValue;
            
            return boost;
        }
    }
}