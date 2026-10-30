using System;
using System.Collections;
using JukeboxCore.Models.Preferences;
using JukeboxCore.Playback;
using JukeboxCore.Songs;
using JukeboxCore.Songs.Models;
using UnityEngine;

namespace Jukebox.Components.Player;

public class JukeboxTrackPlayer(JukeboxMusicChanger changer, PlaybackState state, Action<JukeboxSong, bool> notifyClip)
{
    private static bool AlwaysPlayIntro => PrefsManager.Instance!.GetBoolLocal("jukebox.alwaysPlayIntro");

    private AudioSource Source => changer.battle;

    public IEnumerator Play(
        JukeboxSongSession session,
        bool isFirstSong,
        JukeboxPreferences.PlayStartingFrom trackToResume,
        Func<bool> interrupted,
        Func<bool> loopOne)
    {
        var wait = new WaitUntil(() => (!Source.isPlaying && Application.isFocused) || interrupted());

        state.ClipCount = session.Clips.Count;

        var song = session.Song;
        var newSong = true;
        var playIntro = isFirstSong || AlwaysPlayIntro;
        var forcePlayIntro = trackToResume == null || trackToResume.Clip == 0;

        if (playIntro && session.IntroClip != null && forcePlayIntro)
        {
            state.ClipIndex = 0;
            state.Subtitles = session.IntroSubtitles;
            if (trackToResume != null)
            {
                changer.ChangeTo(session.IntroClip, session.CalmIntroClip, trackToResume.StartFrom);
                trackToResume = null;
            }
            else
            {
                changer.ChangeTo(session.IntroClip, session.CalmIntroClip);
            }

            notifyClip(song, true);
            newSong = false;
            yield return wait;
        }

        if (interrupted())
            yield break;

        do
        {
            var startFromClip = trackToResume?.Clip != 0
                                && session.Clips.Count >= trackToResume?.Clip
                                && session.Clips[trackToResume.Clip - 1].length > trackToResume.StartFrom
                ? trackToResume.Clip - 1
                : 0;

            for (var i = startFromClip; i < session.Clips.Count; i++)
            {
                state.ClipIndex = i + 1;
                state.Subtitles = session.Subtitles;
                if (interrupted())
                    break;

                var calmClip = session.CalmClips?.Count > i ? session.CalmClips[i] : null;

                if (trackToResume != null)
                {
                    changer.ChangeTo(session.Clips[i], calmClip, trackToResume.StartFrom);
                    trackToResume = null;
                }
                else
                {
                    changer.ChangeTo(session.Clips[i], calmClip);
                }

                notifyClip(song, newSong);
                newSong = false;
                yield return wait;
                
                if (interrupted())
                    break;
            }
        } while (loopOne() && !interrupted());
    }
}
