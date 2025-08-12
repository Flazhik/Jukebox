using System.IO;
using JukeboxCore.Models.Preferences;
using UnityEngine;
using static Newtonsoft.Json.JsonConvert;

namespace Jukebox.Components
{
    public class JukeboxVolumeManager: MonoSingleton<JukeboxVolumeManager>
    {
        private static readonly string PreferencesPath = Path.Combine(
            Directory.GetParent(Application.dataPath)!.FullName,
            "Preferences", "JukeboxVolumeSettings.json");

        private PrefsManager prefsManager;
        private JukeboxVolumeConfig preferences = new();
        private bool isDirty;
        
        protected override void Awake()
        {
            base.Awake();
            prefsManager = PrefsManager.Instance;
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

        public float? GetVolumeBoostFor(string trackPath)
        {
            if (preferences.boostSettings.TryGetValue(trackPath, out var boost))
                return boost;
            
            return null;
        }
        
        public void SetVolumeBoostFor(string trackPath, float value)
        {
            preferences.boostSettings[trackPath] = value;
            isDirty = true;
        }

        private void LoadPreferences()
        {
            JukeboxVolumeConfig deserializedConfig;
            using (var streamReader = new StreamReader(File.Open(PreferencesPath, FileMode.OpenOrCreate)))
                deserializedConfig = DeserializeObject<JukeboxVolumeConfig>(streamReader.ReadToEnd());

            if (deserializedConfig?.boostSettings == null)
                SavePreferences();
            else
                preferences = deserializedConfig;
        }
        
        private void SavePreferences() => File.WriteAllText(PreferencesPath, SerializeObject(preferences));
    }
}