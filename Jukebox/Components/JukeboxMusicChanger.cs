using System.Collections;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Jukebox.Components;

public class JukeboxMusicChanger : MonoBehaviour
{
    private const int FadeSpeed = 1;

    public AudioSource clean;
    public AudioSource battle;

    private ThemeVariation currentTheme = ThemeVariation.Battle;
    private JukeboxThemesManager themesManager;
    private Coroutine updateRoutine;

    private static bool CalmThemesAllowed => PrefsManager.Instance.GetBoolLocal("jukebox.enableCalmThemes");

    public void Awake()
    {
        themesManager = JukeboxThemesManager.Instance;
    }

    private void Update()
    {
        if (clean.clip == null)
            return;

        if (currentTheme == ThemeVariation.Calm)
        {
            clean.volume = Mathf.MoveTowards(clean.volume, 1, FadeSpeed * Time.deltaTime);
            battle.volume = Mathf.MoveTowards(battle.volume, 0, FadeSpeed * Time.deltaTime);
        }
        else
        {
            clean.volume = Mathf.MoveTowards(clean.volume, 0.0f, FadeSpeed * Time.deltaTime);
            battle.volume = Mathf.MoveTowards(battle.volume, 1, FadeSpeed * Time.deltaTime);
        }
    }

    public void ChangeTo(AudioClip battleTheme, AudioClip calmTheme, float startingFrom = 0f)
    {
        if (updateRoutine != null)
            StopCoroutine(updateRoutine);

        clean.clip = calmTheme;
        battle.clip = battleTheme;

        battle.Play();
        battle.time = startingFrom;
        battle.volume = 1f;
        if (clean.clip != null)
        {
            clean.Play();
            clean.time = startingFrom;
            if (CalmThemesAllowed && themesManager.FewEnemies)
            {
                clean.volume = 1f;
                battle.volume = 0f;
            }
        }

        updateRoutine = StartCoroutine(SlowUpdate());
    }

    public void RewindTo(float position = 0f)
    {
        Seek(clean, position);
        Seek(battle, position);
        return;

        void Seek(AudioSource source, float pos)
        {
            if (source == null || source.clip == null)
                return;

            var samples = Mathf.RoundToInt(Mathf.Clamp(pos, 0f, source.clip.length) * source.clip.frequency);
            source.timeSamples = Mathf.Clamp(samples, 0, source.clip.samples - 1);
        }
    }

    [SuppressMessage("ReSharper", "IteratorNeverReturns")]
    private IEnumerator SlowUpdate()
    {
        while (true)
        {
            if (CalmThemesAllowed && themesManager.FewEnemies)
                PlayCalmTheme();
            else
                PlayBattleTheme();
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }

    private void PlayBattleTheme()
    {
        if (currentTheme == ThemeVariation.Battle)
            return;

        currentTheme = ThemeVariation.Battle;
        if (clean.clip != null)
            battle.timeSamples = clean.timeSamples;
    }

    private void PlayCalmTheme()
    {
        if (currentTheme == ThemeVariation.Calm)
            return;

        currentTheme = ThemeVariation.Calm;

        if (clean.clip != null)
            clean.timeSamples = battle.timeSamples;
    }

    private enum ThemeVariation
    {
        Battle,
        Calm
    }
}
