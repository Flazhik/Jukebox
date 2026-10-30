using System;
using System.Collections;
using System.Collections.Generic;
using Jukebox.Components.Player;
using Jukebox.Input;
using JukeboxCore.Components;
using JukeboxCore.Events;
using JukeboxCore.Models.Preferences;
using JukeboxCore.Playback;
using JukeboxCore.Songs;
using JukeboxCore.Songs.Models;
using JukeboxCore.Subtitles;
using UnityEngine;
using UnityEngine.InputSystem;
using static Playlist;
using static Playlist.LoopMode;
using static JukeboxCore.Logging.JukeboxLogger;

namespace Jukebox.Components;

public class JukeboxMusicPlayer : MonoBehaviour, IPlaybackController
{
    [SerializeField] private JukeboxPlaylistEditor playlistEditor;
    [SerializeField] private JukeboxSongsLoader loader;
    [SerializeField] private JukeboxMusicChanger changer;
    
    public PlaybackState State { get; } = new();
    public AudioSource Source => changer.battle;

    public event Action<ClipChangedArgs> OnNextAudioClip;
    public event Action<IEnumerable<SongIdentifier>> OnOrderChange;
    public event Action OnStop;
    public event Action<string> OnSubtitleChanged;
    
    private static float VolumeBoostValue => PrefsManager.Instance!.GetFloatLocal("jukebox.volumeBoost");
    private static bool IndividualBoostPerTrack => PrefsManager.Instance!.GetBoolLocal("jukebox.individualBoostPerTrack");
    private static bool ResumeLastSong => PrefsManager.Instance!.GetBoolLocal("jukebox.resumeLastSong");

    private PlaybackOrder playbackOrder;
    private JukeboxTrackPlayer trackPlayer;
    private Coroutine currentPlaylistRoutine;
    private JukeboxSongSession currentSession;
    private Command pendingPlayerCommand;
    private int jumpTo;
    private bool allowVolumeBoost;
    private SubtitleTrack lastSubtitleTrack;
    private int subtitleIndex = -1;
    private Coroutine rewindRoutine;

    public void Awake()
    {
        changer = gameObject.GetComponent<JukeboxMusicChanger>();
        trackPlayer = new JukeboxTrackPlayer(changer, State, NextAudioClip);
        PrefsManager.onPrefChanged += OnPrefChanged;
        
        JukeboxInputs.Instance!.DisablePlayer.performed += OnPlayerDisabled;
        JukeboxInputs.Instance!.NextTrack.performed += OnNextTrack;
        JukeboxInputs.Instance!.RandomTrack.performed += OnRandomTrack;
    }

    public void OnEnable()
    {
        StartPlaylist();
    }

    private void OnDisable()
    {
        if (!ResumeLastSong || State.Song == null)
            return;

        var preferences = JukeboxPreferencesManager.Instance;
        
        if (preferences == null)
            return;
        
        preferences.SetPlaybackPosition(State.Song.Id, State.ClipIndex, Source.time);
        preferences.ForceSavePreferences();
    }

    public void OnDestroy()
    {
        PrefsManager.onPrefChanged -= OnPrefChanged;
        JukeboxInputs.Instance!.DisablePlayer.performed -= OnPlayerDisabled;
        JukeboxInputs.Instance.NextTrack.performed -= OnNextTrack;
        JukeboxInputs.Instance.RandomTrack.performed -= OnRandomTrack;
        StopCurrentRoutines();
        State.Song = null;
    }

    private void Update()
    {
        var track = State.Subtitles;
        if (!ReferenceEquals(track, lastSubtitleTrack))
        {
            lastSubtitleTrack = track;
            subtitleIndex = -1;
        }

        var index = track?.IndexAt(Source.time, subtitleIndex) ?? -1;
        if (index == subtitleIndex)
            return;

        subtitleIndex = index;
        State.SubtitleLine = index < 0 ? null : track.Lines[index].Text;
        OnSubtitleChanged?.Invoke(State.SubtitleLine);
    }

    public void Stop()
    {
        State.Stopped = true;
    }

    public void ChangeTrack(int index)
    {
        jumpTo = index;
        pendingPlayerCommand = Command.JumpTo;
    }

    public void RewindTo(float position)
    {
        if (rewindRoutine != null)
            StopCoroutine(rewindRoutine);

        rewindRoutine = StartCoroutine(RewindRoutine(position));
    }

    private IEnumerator RewindRoutine(float position)
    {
        if (currentSession is not JukeboxCustomSongSession session || State.ClipIndex < 0)
        {
            changer.RewindTo(position);
            rewindRoutine = null;
            yield break;
        }

        if (session.IsFullyLoaded(State.ClipIndex))
        {
            changer.RewindTo(position);
            rewindRoutine = null;
            yield break;
        }

        AudioClip battleClip = null;
        AudioClip calmClip = null;

        yield return session.LoadFullyFor(State.ClipIndex, (battle, calm) =>
        {
            battleClip = battle;
            calmClip = calm;
        });

        if (battleClip == null)
        {
            changer.RewindTo(position);
            rewindRoutine = null;
            yield break;
        }

        changer.ChangeTo(battleClip, calmClip, position);
        rewindRoutine = null;
    }

    public void StartPlaylist()
    {
        if (playlistEditor.playlist.Count < 1)
        {
            Log.LogError("No songs in playlist, somehow. Not starting playlist routine...");
            return;
        }

        StopCurrentRoutines();
        State.Stopped = false;
        pendingPlayerCommand = Command.None;
        playbackOrder = new PlaybackOrder(playlistEditor.playlist);
        currentPlaylistRoutine = StartCoroutine(PlaylistRoutine());
    }

    public void StopPlaylist()
    {
        Source.Stop();
        StopCurrentRoutines();
        State.Song = null;
        State.Subtitles = null;
        State.Stopped = true;
        pendingPlayerCommand = Command.None;
        State.ClipIndex = -1;
        State.ClipCount = 0;
    }

    private void StopCurrentRoutines()
    {
        if (currentPlaylistRoutine != null)
        {
            StopCoroutine(currentPlaylistRoutine);
            currentPlaylistRoutine = null;
        }

        if (rewindRoutine != null)
        {
            StopCoroutine(rewindRoutine);
            rewindRoutine = null;
        }

        currentSession?.Dispose();
        currentSession = null;
    }

    private IEnumerator PlaylistRoutine()
    {
        var preferences = JukeboxPreferencesManager.Instance!;
        var resume = preferences.GetPlaybackPosition();
        preferences.ResetPlaybackPosition();

        var first = true;
        while (!State.Stopped)
        {
            var order = playbackOrder.BuildOrder();
            OnOrderChange?.Invoke(order);

            var index = playbackOrder.StartIndex(order);
            JukeboxPreferences.PlayStartingFrom startFrom = null;

            if (first && ResumeLastSong && resume != null)
            {
                var resumeId = new SongIdentifier(resume.Path, resume.Type);
                var resumeIndex = order.FindIndex(id => Equals(id, resumeId));
                if (resumeIndex >= 0 && loader.Get(resumeId) != null)
                {
                    index = resumeIndex;
                    startFrom = resume;
                    resume = null;
                }
            }

            var unplayable = 0;
            while (!State.Stopped)
            {
                State.SongIndex = index;

                var session = loader.CreateSession(loader.Get(order[index]));
                if (session == null)
                {
                    Log.LogWarning($"Unable to load {order[index].path}, skipping");

                    if (++unplayable >= order.Count)
                    {
                        Log.LogError("No playable tracks in the playlist, stopping");
                        State.Stopped = true;
                        break;
                    }

                    var skip = ResolveNext(index, order.Count);
                    if (skip < 0)
                        break;

                    index = skip;
                    continue;
                }

                unplayable = 0;
                currentSession = session;
                PresenceController.UpdateCyberGrindWave(EndlessGrid.Instance!.currentWave);
                yield return session.Load(trackPlayer.Play(session, first, startFrom, Interrupted, IsLoopOne));
                
                currentSession = null;
                first = false;
                startFrom = null;

                var next = ResolveNext(index, order.Count);
                if (next < 0)
                    break;

                index = next;
            }
        }
    }
    
    private int ResolveNext(int cursor, int count)
    {
        var next = pendingPlayerCommand switch
        {
            Command.RandomTrack => PlaybackOrder.RandomIndex(count),
            Command.JumpTo => jumpTo,
            _ => cursor + 1
        };

        pendingPlayerCommand = Command.None;
        return next < count ? next : -1;
    }

    private bool Interrupted() => pendingPlayerCommand != Command.None || State.Stopped;

    private bool IsLoopOne() => playlistEditor.playlist.loopMode == LoopOne;

    private void OnNextTrack(InputAction.CallbackContext _) => pendingPlayerCommand = Command.NextTrack;

    private void OnRandomTrack(InputAction.CallbackContext _) => pendingPlayerCommand = Command.RandomTrack;

    private void OnPlayerDisabled(InputAction.CallbackContext _)
    {
        if (!State.Stopped)
        {
            StopPlaylist();
            OnStop?.Invoke();
        }
        else
        {
            StartPlaylist();
        }
    }

    private void OnPrefChanged(string key, object value)
    {
        switch (key)
        {
            case "jukebox.volumeBoost":
            {
                if (IndividualBoostPerTrack && State.Song is { IsCustom: true })
                    JukeboxPreferencesManager.Instance?.SetVolumeBoostFor(State.Song.Id.path, (float)value);
                State.VolumeBoost = (float)value;
                break;
            }
            case "jukebox.individualBoostPerTrack":
                State.VolumeBoost = CalculateBoostFor(State.Song);
                break;
        }
    }

    private void NextAudioClip(JukeboxSong song, bool newSong)
    {
        State.Song = song;
        allowVolumeBoost = song.IsCustom;
        State.VolumeBoost = CalculateBoostFor(song);
        OnNextAudioClip?.Invoke(new ClipChangedArgs(song, newSong));
    }

    private float CalculateBoostFor(JukeboxSong song)
    {
        if (!allowVolumeBoost)
            return 0;

        if (!IndividualBoostPerTrack
            || song == null
            || JukeboxPreferencesManager.Instance!.GetVolumeBoostFor(song.Id.path) is not { } boost)
            return VolumeBoostValue;

        return boost;
    }

    private enum Command
    {
        NextTrack,
        RandomTrack,
        JumpTo,
        None,
    }
}
