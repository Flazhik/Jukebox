using System.IO;
using JukeboxCore.Models.Preferences;
using UnityEngine;
using static Newtonsoft.Json.JsonConvert;

namespace Jukebox.Components
{
    [ConfigureSingleton(SingletonFlags.HideAutoInstance)]
    public class JukeboxPreferencesManager: MonoSingleton<JukeboxPreferencesManager>
    {
        private static readonly string PreferencesPath = Path.Combine(
            Directory.GetParent(Application.dataPath)!.FullName,
            "Preferences", "JukeboxVolumeSettings.json");

        private PrefsManager prefsManager;
        private JukeboxPreferences preferences = new();
        private bool isDirty;
        
        protected void Awake()
        {
            prefsManager = PrefsManager.Instance;
            LoadPreferences();
            InvokeRepeating(nameof(SlowUpdate), 0.0f, 3.0f);
        }
        
        private void SlowUpdate()
        {
            if (!isDirty)
                return;
            
            SavePreferences();
        }

        protected void OnDestroy()
        {
            SavePreferences();
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
        
        public void SetPlaybackPosition(Playlist.SongIdentifier id, int clip, float value)
        {
            preferences.playTrackStaringFrom = new JukeboxPreferences.PlayStartingFrom(id, clip, value);
            isDirty = true;
        }
        
        public JukeboxPreferences.PlayStartingFrom GetPlaybackPosition() => preferences.playTrackStaringFrom;
        
        public void ResetPlaybackPosition()
        {
            preferences.playTrackStaringFrom = null;
            isDirty = true;
        }

        public void ForceSavePreferences() => SavePreferences();

        private void LoadPreferences()
        {
            JukeboxPreferences deserializedConfig;
            using (var streamReader = new StreamReader(File.Open(PreferencesPath, FileMode.OpenOrCreate)))
                deserializedConfig = DeserializeObject<JukeboxPreferences>(streamReader.ReadToEnd());

            if (deserializedConfig?.boostSettings == null)
                SavePreferences();
            else
                preferences = deserializedConfig;
        }

        private void SavePreferences()
        {
            File.WriteAllText(PreferencesPath, SerializeObject(preferences));
            isDirty = false;
        }
    }
}