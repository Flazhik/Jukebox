using System.Collections;
using JukeboxCore.Components;
using JukeboxCore.Songs;
using JukeboxCore.Songs.Models;
using UnityEngine;
using static JukeboxCore.Utils.ReflectionUtils;

namespace Jukebox.Components;

public class PreviewHelper : MonoBehaviour
{
    public bool inZone;

    private Coroutine currentRoutine;
    private JukeboxSongSession currentSession;
    private AudioSource preview;
    private ScreenZone screenZone;
    private bool stop;
    private AudioSource terminalMusic;

    private void Awake()
    {
        screenZone = GetComponent<ScreenZone>();
        terminalMusic = GetPrivate<AudioSource>(screenZone, typeof(ScreenZone), "music");
        inZone = GetPrivate<bool>(screenZone, typeof(ScreenZone), "inZone");
        preview = gameObject.AddComponent<AudioSource>();
        preview.outputAudioMixerGroup = AudioMixerController.Instance!.musicGroup;
    }

    private void OnDestroy()
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentSession?.Dispose();
        currentSession = null;
    }

    private void Update()
    {
        if (preview.clip == null)
            return;

        if (inZone)
        {
            if (preview.pitch < 1.0)
                preview.pitch = Mathf.MoveTowards(preview.pitch, 1f, Time.deltaTime);

            if (!stop)
                return;

            preview.volume = Mathf.MoveTowards(preview.volume, 0.0f, Time.deltaTime);
        }
        else if (preview.pitch > 0.0)
        {
            preview.pitch = Mathf.MoveTowards(preview.pitch, 0.0f, Time.deltaTime);
        }
    }

    public void PlayPreview(JukeboxSong song)
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }

        currentSession?.Dispose();
        currentSession = JukeboxSongsLoader.Instance?.CreateSession(song);

        if (currentSession == null)
            return;

        currentRoutine = StartCoroutine(currentSession.Load(PreviewCoroutine(currentSession)));
    }

    private IEnumerator PreviewCoroutine(JukeboxSongSession session)
    {
        stop = false;
        terminalMusic.Pause();
        preview.clip = session.Clips[0];
        preview.volume = 1;
        preview.Play();
        
        var mixerController = AudioMixerController.Instance;
        
        // idk what's causing mixergroup to mute audiosource until the volume is changed
        mixerController?.SetMusicVolume(1);
        mixerController?.SetMusicVolume(mixerController.optionsMusicVolume);
        
        yield return new WaitForSeconds(5);
        stop = true;
        yield return new WaitUntil(() => preview.volume <= 0f);
        preview.Stop();
        terminalMusic.Play();
    }
}
