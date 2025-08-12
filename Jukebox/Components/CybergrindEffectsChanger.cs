using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using static Jukebox.Utils.ReflectionUtils;
using static UnityEngine.GameObject;
namespace Jukebox.Components
{
    public class CybergrindEffectsChanger: MonoBehaviour
    {
        private const string TerminalPath = "/FirstRoom/Room/Cybergrind Shop/Music";
        
        private CrowdReactions crowdReactions;
        private PrefsManager prefsManager;

        private AudioClip awwDefault;
        private AudioClip cheerDefault;
        private AudioClip cheerLongDefault;
        private AudioClip terminalMusicDefault;
        private AudioClip gameOverDefault;
        
        private static AudioSource EndMusicAudioSource => SceneManager.GetActiveScene()
            .GetRootGameObjects()
            .First(o => o.name == "EndMusic")
            .GetComponent<AudioSource>();

        private static AudioSource MenuMusicAudioSource => Find(TerminalPath).GetComponent<AudioSource>();
        
        private void Awake()
        {
            crowdReactions = CrowdReactions.Instance;
            prefsManager = PrefsManager.Instance;

            awwDefault = Instantiate(crowdReactions.aww, gameObject.transform);
            cheerDefault = Instantiate(crowdReactions.cheer, gameObject.transform);
            cheerLongDefault = Instantiate(crowdReactions.cheerLong, gameObject.transform);
            terminalMusicDefault = Instantiate(MenuMusicAudioSource.clip, gameObject.transform);
            gameOverDefault = Instantiate(EndMusicAudioSource.clip, gameObject.transform);
            
            StartCoroutine(Setup());
            PrefsManager.onPrefChanged += OnPrefChanged;
        }
        
        private void OnDestroy()
        {
            PrefsManager.onPrefChanged -= OnPrefChanged;
        }

        private IEnumerator Setup()
        {
            yield return PrepareParrySfx();
            yield return PrepareWaveCompleteSfx();
            yield return PrepareAwwSfx();
            yield return PrepareGameOverSfx();
            yield return PrepareTerminalMusic();
        }

        private IEnumerator PrepareParrySfx()
        {
            yield return PrepareClip(prefsManager.GetStringLocal("jukebox.effects.parry"), clip =>
                crowdReactions.cheer = clip, cheerDefault);
        }
        
        private IEnumerator PrepareWaveCompleteSfx()
        {
            yield return PrepareClip(prefsManager.GetStringLocal("jukebox.effects.wavecomplete"), clip =>
                crowdReactions.cheerLong = clip, cheerLongDefault);
        }
        
        private IEnumerator PrepareAwwSfx()
        {
            yield return PrepareClip(prefsManager.GetStringLocal("jukebox.effects.aww"), clip =>
                crowdReactions.aww = clip, awwDefault);
        }

        private IEnumerator PrepareGameOverSfx()
        {
            yield return PrepareClip(prefsManager.GetStringLocal("jukebox.effects.gameover"), clip =>
                EndMusicAudioSource.clip = clip, gameOverDefault);
        }
        
        private IEnumerator PrepareTerminalMusic()
        {
            var terminalMusic = MenuMusicAudioSource;
            yield return PrepareClip(prefsManager.GetStringLocal("jukebox.effects.terminal-music"), clip =>
            {
                terminalMusic.Stop();
                terminalMusic.volume = 1f;
                terminalMusic.clip = clip;
                terminalMusic.Play();
            }, terminalMusicDefault);
        }

        private IEnumerator PrepareClip(string path, Action<AudioClip> callback, AudioClip fallback)
        {
            if (path != null)
                yield return Download(new FileInfo(path), callback);
            else
                callback(fallback);
        }

        private IEnumerator Download(FileSystemInfo path, Action<AudioClip> callback)
        {
            var handler = new DownloadHandlerAudioClip(new Uri(path.FullName).AbsoluteUri,
                CustomMusicFileBrowser.extensionTypeDict[path.Extension.ToLower()]);

            using var wr = new UnityWebRequest(new Uri(path.FullName).AbsoluteUri, "GET", handler, null);
            yield return wr.SendWebRequest();
            if (wr.responseCode == 200)
                callback(handler.audioClip);
        }
        
        private void OnPrefChanged(string key, object value)
        {
            switch (key)
            {
                case "jukebox.effects.wavecomplete":
                    StartCoroutine(PrepareWaveCompleteSfx());
                    break;
                case "jukebox.effects.aww":
                    StartCoroutine(PrepareAwwSfx());
                    break;
                case "jukebox.effects.gameover":
                    StartCoroutine(PrepareGameOverSfx());
                    break;
                case "jukebox.effects.terminal-music":
                    StartCoroutine(PrepareTerminalMusic());
                    break;
                case "jukebox.effects.parry":
                    var audioSource = GetPrivate<AudioSource>(crowdReactions, typeof(CrowdReactions), "aud");
                    audioSource.Stop();
                    StartCoroutine(PrepareParrySfx());
                    break;
            }
        }
    }
}