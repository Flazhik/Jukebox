using System.Collections;
using JukeboxCore.Models.Song;
using UnityEngine;
using static Jukebox.Utils.ReflectionUtils;

namespace Jukebox.Components
{
    public class PreviewHelper: MonoBehaviour
    {
        public bool inZone;
        
        private Coroutine currentRoutine;
        private ScreenZone screenZone;
        private AudioSource terminalMusic;
        private bool stop;
        private AudioSource preview;

        private void Awake()
        {
            screenZone = GetComponent<ScreenZone>();
            terminalMusic = GetPrivate<AudioSource>(screenZone, typeof(ScreenZone), "music");
            inZone = GetPrivate<bool>(screenZone, typeof(ScreenZone), "inZone");
            preview = gameObject.AddComponent<AudioSource>();
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
                preview.pitch = Mathf.MoveTowards(preview.pitch, 0.0f, Time.deltaTime);
        }

        public void PlayPreview(JukeboxSong song)
        {
            if (currentRoutine != null)
                StopCoroutine(currentRoutine);

            currentRoutine = StartCoroutine(song.Acquire(PreviewCoroutine(song)));
        }

        private IEnumerator PreviewCoroutine(JukeboxSong song)
        {
            stop = false;
            terminalMusic.Pause();
            preview.clip = song.Clips[0];
            preview.volume = 1;
            preview.Play();
            yield return new WaitForSeconds(5);
            stop = true;
            yield return new WaitUntil(() => preview.volume <= 0f);
            preview.Stop();
            terminalMusic.Play();
        }
    }
}