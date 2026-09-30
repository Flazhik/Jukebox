using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using static JukeboxCore.Utils.ReflectionUtils;
using static UnityEngine.GameObject;
using static JukeboxCore.Logging.JukeboxLogger;

namespace Jukebox.Components;

public class CybergrindEffectsChanger : MonoBehaviour
{
    private const string TerminalPath = "/FirstRoom/Room/Cybergrind Shop/Music";
    private const string ParryParam = "jukebox.effects.parry";
    private const string WaveParam = "jukebox.effects.wavecomplete";
    private const string AwwParam = "jukebox.effects.aww";
    private const string GameOverParam = "jukebox.effects.gameover";
    private const string TerminalMusicParam = "jukebox.effects.terminal-music";

    private AudioClip awwDefault;
    private AudioClip cheerDefault;
    private AudioClip cheerLongDefault;
    private AudioClip gameOverDefault;
    private AudioClip terminalMusicDefault;
    private CrowdReactions crowdReactions;
    private PrefsManager prefsManager;

    private AudioSource EndMusicSource
    {
        get
        {
            if (field == null)
                field = SceneManager.GetActiveScene()
                    .GetRootGameObjects()
                    .First(static o => o.name == "EndMusic")
                    .GetComponent<AudioSource>();

            return field;
        }
    }

    private AudioSource MenuMusicSource
    {
        get
        {
            if (field == null)
                field = Find(TerminalPath).GetComponent<AudioSource>();

            return field;
        }
    }

    private AudioSource CrowdAudioSource
    {
        get
        {
            if (field == null)
                field = GetPrivate<AudioSource>(crowdReactions, typeof(CrowdReactions), "aud");

            return field;
        }
    }

    private void Awake()
    {
        crowdReactions = CrowdReactions.Instance;
        prefsManager = PrefsManager.Instance;

        awwDefault = Instantiate(crowdReactions?.aww, gameObject.transform);
        cheerDefault = Instantiate(crowdReactions?.cheer, gameObject.transform);
        cheerLongDefault = Instantiate(crowdReactions?.cheerLong, gameObject.transform);
        terminalMusicDefault = Instantiate(MenuMusicSource.clip, gameObject.transform);
        gameOverDefault = Instantiate(EndMusicSource.clip, gameObject.transform);

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

    private IEnumerator PrepareParrySfx() =>
        PrepareClip(prefsManager.GetStringLocal(ParryParam), clip => crowdReactions.cheer = clip,
            cheerDefault);

    private IEnumerator PrepareWaveCompleteSfx() =>
        PrepareClip(prefsManager.GetStringLocal(WaveParam), clip => crowdReactions.cheerLong = clip,
            cheerLongDefault);

    private IEnumerator PrepareAwwSfx() =>
        PrepareClip(prefsManager.GetStringLocal(AwwParam), clip => crowdReactions.aww = clip,
            awwDefault);

    private IEnumerator PrepareGameOverSfx() =>
        PrepareClip(prefsManager.GetStringLocal(GameOverParam), clip => EndMusicSource.clip = clip,
            gameOverDefault);

    private IEnumerator PrepareTerminalMusic()
    {
        var terminalMusic = MenuMusicSource;
        return PrepareClip(prefsManager.GetStringLocal(TerminalMusicParam), clip =>
        {
            terminalMusic.Stop();
            terminalMusic.volume = 1f;
            terminalMusic.clip = clip;
            terminalMusic.Play();
        }, terminalMusicDefault);
    }

    private IEnumerator PrepareClip(string path, Action<AudioClip> callback, AudioClip fallback)
    {
        if (string.IsNullOrEmpty(path))
            callback(fallback);
        else
            yield return Download(new FileInfo(path), callback);
    }

    private IEnumerator Download(FileSystemInfo path, Action<AudioClip> callback)
    {
        var uri = new Uri(path.FullName).AbsoluteUri;
        if (!CustomMusicFileBrowser.extensionTypeDict.TryGetValue(path.Extension.ToLower(), out var audioType))
        {
            Log.LogWarning($"Unsupported audio file extension: {path.Extension}");
            yield break;
        }

        var handler = new DownloadHandlerAudioClip(uri, audioType);
        using var wr = new UnityWebRequest(uri, "GET", handler, null);
        yield return wr.SendWebRequest();
        if (wr.responseCode == 200)
            callback(handler.audioClip);
    }

    private void OnPrefChanged(string key, object value)
    {
        switch (key)
        {
            case WaveParam:
                StartCoroutine(PrepareWaveCompleteSfx());
                break;
            case AwwParam:
                StartCoroutine(PrepareAwwSfx());
                break;
            case GameOverParam:
                StartCoroutine(PrepareGameOverSfx());
                break;
            case TerminalMusicParam:
                StartCoroutine(PrepareTerminalMusic());
                break;
            case ParryParam:
                CrowdAudioSource.Stop();
                StartCoroutine(PrepareParrySfx());
                break;
        }
    }
}
