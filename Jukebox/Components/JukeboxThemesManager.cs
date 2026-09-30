using System.IO;
using System.Linq;
using JukeboxCore.Models.Preferences;
using JukeboxCore.Utils;
using UnityEngine;

namespace Jukebox.Components;

[ConfigureSingleton(SingletonFlags.HideAutoInstance)]
public class JukeboxThemesManager : MonoSingleton<JukeboxThemesManager>
{
    private static readonly string PreferencesPath = Path.Combine(
        Directory.GetParent(Application.dataPath)!.FullName,
        "Preferences", "JukeboxThemes.json");

    private EnemyTracker enemyTracker;
    private PrefsManager prefsManager;
    private JsonStore<JukeboxThemesConfig> store;

    public bool FewEnemies => EnemiesAlive <= CalmThemeThreshold && SpecialEnemiesThresholdReached();
    private int CalmThemeThreshold => prefsManager.GetIntLocal("jukebox.calmThemeEnemiesThreshold");
    private int EnemiesAlive => enemyTracker.enemies.Count(static enemy => !enemy.dead);

    protected void Awake()
    {
        prefsManager = PrefsManager.Instance;
        enemyTracker = EnemyTracker.Instance;

        store = new JsonStore<JukeboxThemesConfig>(PreferencesPath);
        if (store.Loaded)
            store.Mutate(static config => { config.Equalize(); });
        else
            store.Save();

        InvokeRepeating(nameof(SlowUpdate), 0.0f, 3.0f);
    }

    protected void OnDestroy()
    {
        store.Save();
    }

    private void SlowUpdate()
    {
        store.SaveIfDirty();
    }

    public int? GetThresholdFor(EnemyType type, bool radiant = false)
    {
        var enemies = radiant
            ? store.Value.calmTheme.specialRadiantEnemies
            : store.Value.calmTheme.specialEnemies;

        return enemies.TryGetValue(type, out var threshold) ? threshold : null;
    }

    public void SetThresholdFor(EnemyType type, int value, bool radiant = false)
    {
        var enemies = radiant
            ? store.Value.calmTheme.specialRadiantEnemies
            : store.Value.calmTheme.specialEnemies;

        enemies[type] = value;
        store.MarkDirty();
    }

    public void DisableThresholdFor(EnemyType type, bool radiant)
    {
        var enemies = radiant
            ? store.Value.calmTheme.specialRadiantEnemies
            : store.Value.calmTheme.specialEnemies;

        enemies.Remove(type);
        store.MarkDirty();
    }

    private bool SpecialEnemiesThresholdReached()
    {
        return enemyTracker.enemies
            .Where(static enemy => !enemy.dead)
            .GroupBy(static enemy => new { enemy.enemyType, enemy.hasRadianceEffected })
            .All(group =>
            {
                if (group.Key.hasRadianceEffected)
                    return !store.Value.calmTheme.specialRadiantEnemies.ContainsKey(group.Key.enemyType)
                           || store.Value.calmTheme.specialRadiantEnemies[group.Key.enemyType] >=
                           group.Count();

                return !store.Value.calmTheme.specialEnemies.ContainsKey(group.Key.enemyType)
                       || store.Value.calmTheme.specialEnemies[group.Key.enemyType] >=
                       group.Count();
            });
    }
}