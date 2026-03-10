using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Jukebox.Input;
using JukeboxCore.Components;
using JukeboxCore.Events;
using JukeboxCore.Models.Preferences;
using JukeboxCore.Models.Song;
using UnityEngine;
using UnityEngine.InputSystem;
using static Playlist.LoopMode;
using static Playlist;
using Random = UnityEngine.Random;

namespace Jukebox.Components
{
    public class JukeboxMusicPlayer : MonoBehaviour
    {
        private static float VolumeBoostValue => PrefsManager.Instance.GetFloatLocal("jukebox.volumeBoost");
        private static bool AlwaysPlayIntro => PrefsManager.Instance.GetBoolLocal("jukebox.alwaysPlayIntro");
        private static bool IndividualBoostPerTrack => PrefsManager.Instance.GetBoolLocal("jukebox.individualBoostPerTrack");
        private static bool ResumeLastSong => PrefsManager.Instance.GetBoolLocal("jukebox.resumeLastSong");
        private static bool NextTrackRequested => JukeboxInputs.Instance.NextTrack.WasPerformedThisFrame();
        private static bool RandomTrackRequested => JukeboxInputs.Instance.RandomTrack.WasPerformedThisFrame();
        
        [SerializeField]
        private JukeboxPlaylistEditor playlistEditor;
        
        [SerializeField]
        private JukeboxSongsLoader loader;

        [SerializeField]
        private JukeboxMusicChanger changer;
        
        public static event Action<ClipChangedArgs> OnNextAudioClip;
        public static event Action<IEnumerable<SongIdentifier>> OnOrderChange;
        public static event Action OnStop;

        public AudioSource Source => changer.battle;
        public static JukeboxSong CurrentSong { get; private set; }
        public static int CurrentSongIndex { get; private set; }
        public static int CurrentClipIndex { get; private set; } = -1;
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
            PrefsManager.onPrefChanged -= OnPrefChanged;
            JukeboxInputs.Instance.DisablePlayer.performed -= OnPlayerDisabled;
            CurrentSong = null;
        }

        public void OnEnable() => StartPlaylist();

        private void OnDisable()
        {
            if (NewMovement.Instance.dead || !ResumeLastSong || CurrentSong == null)
                return;
            
            var preferences = JukeboxPreferencesManager.Instance;
            preferences.SetPlaybackPosition(CurrentSong.Id, CurrentClipIndex, Source.time);
            preferences.ForceSavePreferences();
        }

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
            CurrentClipIndex = -1;
        }

        public void ChangeTrack(int index)
        {
            CurrentSongIndex = index;
            forcedChange = true;
        }
        
        public void RewindTo(float position) => changer.RewindTo(position);
        

        private IEnumerator PlaylistRoutine()
        {
            var songFinished = new WaitUntil(() => 
                ((!Source.isPlaying && Application.isFocused)
                || NextTrackRequested
                || RandomTrackRequested
                || forcedChange)
                && !stopped);

            var first = true;
            var playlist = playlistEditor.playlist;
            var shuffled = playlist.shuffled
                ? new DeckShuffled<SongIdentifier>(playlist.ids).AsEnumerable()
                : playlist.ids.AsEnumerable();

            var preferenceManager = JukeboxPreferencesManager.Instance;
            var playbackPosition = preferenceManager.GetPlaybackPosition();
            preferenceManager.ResetPlaybackPosition();
            
            while (!stopped)
            {
                if (shuffled is DeckShuffled<SongIdentifier> deckShuffled)
                    deckShuffled.Reshuffle();

                var currentOrder = shuffled.ToList();
                OnOrderChange?.Invoke(currentOrder);

                JukeboxPreferences.PlayStartingFrom forceStartFrom = null;
                if (ResumeLastSong
                    && playbackPosition != null
                    && currentOrder.Any(id => Equals(id.path, playbackPosition.Path)))
                {
                    var requestedSongId = new SongIdentifier(playbackPosition.Path, playbackPosition.Type);
                    var song = loader.Load(requestedSongId);
                    if (song != null)
                    {
                        forceStartFrom = playbackPosition;
                        CurrentSongIndex = currentOrder.FindIndex(id => Equals(id, requestedSongId));
                    }
                }
                else
                {
                    CurrentSongIndex = playlist.loopMode == LoopOne 
                        ? currentOrder.FindIndex(id => Equals(id, playlist.ids[playlist.selected]))
                        : 0;
                }
                
                for (; CurrentSongIndex < currentOrder.Count; CurrentSongIndex++)
                {
                    forcedChange = false;
                    var id = currentOrder[CurrentSongIndex];
                    var song = loader.Load(id);
                    PresenceController.UpdateCyberGrindWave(EndlessGrid.Instance.currentWave);
                    yield return song.Acquire(Play(first));
                    first = false;

                    IEnumerator Play(bool firstSong)
                    {
                        var newSong = true;
                        var playIntro = firstSong || AlwaysPlayIntro;
                        var forcePlayIntro = forceStartFrom == null || forceStartFrom.Clip == 0;
                        
                        if (playIntro && song.IntroClip != null && forcePlayIntro)
                        {
                            CurrentClipIndex = 0;
                            if (forceStartFrom != null)
                            {
                                changer.ChangeTo(song.IntroClip, song.CalmIntroClip, forceStartFrom.StartFrom);
                                forceStartFrom = null;
                            }
                            else
                                changer.ChangeTo(song.IntroClip, song.CalmIntroClip);

                            NextAudioClip(song, true);
                            newSong = false;
                            yield return songFinished;
                        }

                        if (SwitchTheTrack())
                        {
                            if (RandomTrackRequested)
                                CurrentSongIndex = Random.Range(0, playlist.Count - 1);
                            yield break;
                        }

                        do
                        {
                            var startFromClip = forceStartFrom != null
                                                && forceStartFrom.Clip != 0
                                                && song.Clips.Count >= forceStartFrom.Clip
                                                && song.Clips[forceStartFrom.Clip - 1].length > forceStartFrom.StartFrom
                                ? forceStartFrom.Clip - 1
                                : 0;
                            for (var i = startFromClip; i < song.Clips.Count; i++)
                            {
                                CurrentClipIndex = i + 1;
                                if (stopped)
                                    break;
                                
                                var calmClip = song.CalmClips?.Count > i
                                    ? song.CalmClips[i]
                                    : null;

                                if (forceStartFrom != null)
                                {
                                    changer.ChangeTo(song.Clips[i], calmClip, forceStartFrom.StartFrom);
                                    forceStartFrom = null;
                                }
                                else
                                    changer.ChangeTo(song.Clips[i], calmClip);
                                
                                NextAudioClip(song, newSong);
                                newSong = false;
                                yield return songFinished;
                                if (SwitchTheTrack())
                                {
                                    if (RandomTrackRequested)
                                        CurrentSongIndex = Random.Range(0, playlist.Count - 1);
                                    break;
                                }
                            }
                        } while (playlist.loopMode == LoopOne && !SwitchTheTrack());
                    }

                    bool SwitchTheTrack() => NextTrackRequested || RandomTrackRequested || forcedChange || stopped;
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
                        JukeboxPreferencesManager.Instance.SetVolumeBoostFor(CurrentSong.Id.path, (float) value);
                    VolumeBoost = (float)value;
                    break;
                }
                case "jukebox.individualBoostPerTrack":
                    VolumeBoost = CalculateBoostFor(CurrentSong);
                    break;
            }
        }

        private void NextAudioClip(JukeboxSong song, bool newSong)
        {
            CurrentSong = song;
            allowVolumeBoost = song.IsCustom;
            VolumeBoost = CalculateBoostFor(song);
            OnNextAudioClip?.Invoke(new ClipChangedArgs(song, newSong));
        }

        private float CalculateBoostFor(JukeboxSong song)
        {
            if (!allowVolumeBoost)
                return 0;

            if (!IndividualBoostPerTrack
                || song == null
                || JukeboxPreferencesManager.Instance.GetVolumeBoostFor(song.Id.path) is not { } boost)
                return VolumeBoostValue;

            return boost;
        }
    }
}