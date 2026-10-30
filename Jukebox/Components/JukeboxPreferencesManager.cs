using System.IO;
using JukeboxCore.Models.Preferences;
using JukeboxCore.Utils;
using UnityEngine;

namespace Jukebox.Components;

[ConfigureSingleton(SingletonFlags.HideAutoInstance)]
public class JukeboxPreferencesManager : MonoSingleton<JukeboxPreferencesManager>
{
    private static readonly string PreferencesPath = Path.Combine(
        Directory.GetParent(Application.dataPath)!.FullName,
        "Preferences", "JukeboxVolumeSettings.json");

    private JsonStore<JukeboxPreferences> preferences;

    protected void Awake()
    {
        preferences = new JsonStore<JukeboxPreferences>(PreferencesPath);
        if (!preferences.Loaded)
            preferences.Save();

        InvokeRepeating(nameof(SlowUpdate), 0.0f, 3.0f);
    }

    protected void OnDestroy()
    {
        preferences.Save();
    }

    private void SlowUpdate()
    {
        preferences.SaveIfDirty();
    }
    
    public float? GetVolumeBoostFor(string trackPath) => preferences.Value.boostSettings.TryGetValue(trackPath, out var boost) ? boost : null;

    public void SetVolumeBoostFor(string trackPath, float value)
    {
        preferences.Value.boostSettings[trackPath] = value;
        preferences.MarkDirty();
    }

    public void SetPlaybackPosition(Playlist.SongIdentifier id, int clip, float value)
    {
        preferences.Value.playTrackStaringFrom = new JukeboxPreferences.PlayStartingFrom(id, clip, value);
        preferences.MarkDirty();
    }

    public JukeboxPreferences.PlayStartingFrom GetPlaybackPosition() => preferences.Value.playTrackStaringFrom;
    

    public void ResetPlaybackPosition()
    {
        preferences.Value.playTrackStaringFrom = null;
        preferences.MarkDirty();
    }

    public void ForceSavePreferences()
    {
        preferences.Save();
    }
}