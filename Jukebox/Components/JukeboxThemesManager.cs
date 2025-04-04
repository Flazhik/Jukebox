using System.Collections;
using System.IO;
using System.Linq;
using Jukebox.Core.Model.Themes;
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

        public int? GetThresholdFor(EnemyType type)
        {
            if (!preferences.CalmTheme.SpecialEnemies.ContainsKey(type))
                return null;

            return preferences.CalmTheme.SpecialEnemies[type];
        }
        
        public void SetThresholdFor(EnemyType type, int value)
        {
            preferences.CalmTheme.SpecialEnemies[type] = value;
            isDirty = true;
        }
        
        public void DisableThresholdFor(EnemyType type)
        {
            if (!preferences.CalmTheme.SpecialEnemies.ContainsKey(type))
                return;
            
            preferences.CalmTheme.SpecialEnemies.Remove(type);
            isDirty = true;
        }

        private bool SpecialEnemiesThresholdReached() =>
            enemyTracker.enemies
                .Where(enemy => !enemy.dead)
                .Select(enemy => enemy.enemyType)
                .GroupBy(type => type)
                .All(group => !preferences.CalmTheme.SpecialEnemies.ContainsKey(group.Key) 
                              || preferences.CalmTheme.SpecialEnemies[group.Key] >= group.Count());
        
        private void LoadPreferences()
        {
            JukeboxThemesConfig loaderPreferences;
            using (var streamReader = new StreamReader(File.Open(PreferencesPath, FileMode.OpenOrCreate)))
                loaderPreferences = DeserializeObject<JukeboxThemesConfig>(streamReader.ReadToEnd());

            if (loaderPreferences?.CalmTheme == null)
                SavePreferences();
            else
                preferences = loaderPreferences;
        }
        
        private void SavePreferences() => File.WriteAllText(PreferencesPath, SerializeObject(preferences));
    }
}