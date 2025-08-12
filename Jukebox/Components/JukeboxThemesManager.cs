using System.IO;
using System.Linq;
using JukeboxCore.Models.Preferences;
using UnityEngine;
using static Newtonsoft.Json.JsonConvert;

namespace Jukebox.Components
{
    public class JukeboxThemesManager: MonoSingleton<JukeboxThemesManager>
    {
        private static readonly string PreferencesPath = Path.Combine(
            Directory.GetParent(Application.dataPath)!.FullName,
            "Preferences", "JukeboxThemes.json");

        private PrefsManager prefsManager;
        private EnemyTracker enemyTracker;
        private JukeboxThemesConfig preferences = new();
        
        public bool FewEnemies => EnemiesAlive <= CalmThemeThreshold && SpecialEnemiesThresholdReached();
        private int CalmThemeThreshold => prefsManager.GetIntLocal("jukebox.calmThemeEnemiesThreshold");
        private int EnemiesAlive => enemyTracker.enemies.Count(enemy => !enemy.dead);

        private bool isDirty;

        protected override void Awake()
        {
            base.Awake();
            prefsManager = PrefsManager.Instance;
            enemyTracker = EnemyTracker.Instance;
            LoadPreferences();
            InvokeRepeating(nameof(SlowUpdate), 0.0f, 3.0f);
        }
        
        private void SlowUpdate()
        {
            if (!isDirty)
                return;
            
            SavePreferences();
            isDirty = false;
        }

        protected override void OnDestroy()
        {
            SavePreferences();
            isDirty = false;
            base.OnDestroy();
        }

        public int? GetThresholdFor(EnemyType type, bool radiant = false)
        {
            var enemies = radiant
                ? preferences.calmTheme.specialRadiantEnemies
                : preferences.calmTheme.specialEnemies;
            
            if (!enemies.ContainsKey(type))
                return null;

            return enemies[type];
        }
        
        public void SetThresholdFor(EnemyType type, int value, bool radiant = false)
        {
            var enemies = radiant
                ? preferences.calmTheme.specialRadiantEnemies
                : preferences.calmTheme.specialEnemies;
            
            enemies[type] = value;
            isDirty = true;
        }
        
        public void DisableThresholdFor(EnemyType type, bool radiant)
        {
            var enemies = radiant
                ? preferences.calmTheme.specialRadiantEnemies
                : preferences.calmTheme.specialEnemies;
            
            if (enemies.ContainsKey(type))
                enemies.Remove(type);

            isDirty = true;
        }

        private bool SpecialEnemiesThresholdReached() =>
            enemyTracker.enemies
                .Where(enemy => !enemy.dead)
                .GroupBy(enemy => new { enemy.enemyType, enemy.hasRadianceEffected })
                .All(group =>
                {
                    if (group.Key.hasRadianceEffected)
                        return !preferences.calmTheme.specialRadiantEnemies.ContainsKey(group.Key.enemyType)
                               || preferences.calmTheme.specialRadiantEnemies[group.Key.enemyType] >=
                               group.Count();

                    return !preferences.calmTheme.specialEnemies.ContainsKey(group.Key.enemyType)
                           || preferences.calmTheme.specialEnemies[group.Key.enemyType] >=
                           group.Count();
                });

        private void LoadPreferences()
        {
            JukeboxThemesConfig deserializedConfig;
            using (var streamReader = new StreamReader(File.Open(PreferencesPath, FileMode.OpenOrCreate)))
                deserializedConfig = DeserializeObject<JukeboxThemesConfig>(streamReader.ReadToEnd());

            if (deserializedConfig?.calmTheme == null)
                SavePreferences();
            else
                preferences = deserializedConfig.Equalize();
            
            isDirty = true;
        }
        
        private void SavePreferences() => File.WriteAllText(PreferencesPath, SerializeObject(preferences));
    }
}