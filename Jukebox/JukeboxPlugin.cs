using System.Collections;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using HarmonyLib;
using Jukebox.Components;
using Jukebox.Input;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static JukeboxCore.Utils.PathsUtils;

namespace Jukebox
{
    [BepInProcess("ULTRAKILL.exe")]
    [BepInPlugin(PluginInfo.Guid, PluginInfo.Name, PluginInfo.Version)]
    public class JukeboxPlugin : BaseUnityPlugin
    {
        private static GameObject _bootstrap;

        public static string catalogDir;
        private static Harmony _harmony;
        
        private static bool _motDShown;
        private static bool _init;

        static JukeboxPlugin()
        {
            catalogDir = Path.Combine(AssemblyPath!, "Assets");
        }

        private void Awake()
        {
            Addressables.InitializeAsync().WaitForCompletion();
            Addressables.LoadContentCatalogAsync(Path.Combine(catalogDir, "catalog.json"), true).WaitForCompletion();

            _harmony = new Harmony(PluginInfo.Guid);
            Startup();
        }

        /*
         * Apparently WaitForCompletion() makes addressables go fucky wucky during a scene load causing a deadlock
         * This part serves the only purpose of letting Jukebox bootstrap and action map to be loaded
         * smh
         */
        private void Start()
        {
            _bootstrap = Addressables.LoadAssetAsync<GameObject>("Assets/Jukebox/Bootstrap.prefab")
                .WaitForCompletion();
            Addressables.LoadAssetAsync<InputActionAsset>("Assets/Jukebox/JukeboxControls.inputactions")
                .WaitForCompletion();
            Addressables.LoadAssetAsync<Sprite>("Assets/Jukebox/Textures/music_note.png")
                .WaitForCompletion();
        }

        private void Startup()
        {
            _harmony.PatchAll();
            if (new FileInfo(Path.Combine(AssemblyPath, "cookies.txt")) is var cookiesFile && cookiesFile.Exists)
                cookiesFile.Delete();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (scene != SceneManager.GetActiveScene())
                return;

            switch (SceneHelper.CurrentScene)
            {
                case "Bootstrap":
                {
                    SetDefaultPrefsValues();
                    break;
                }
                case "Main Menu":
                {
                    if (_init)
                        return;
                    
                    _init = true;
                    break;
                }
                case "Endless":
                {
                    var bootstrap = Instantiate(_bootstrap);
                    if (!_motDShown)
                        JukeboxManager.Instance.StartCoroutine(MotD());
                    break;
                }
            }
        }

        private static void SetDefaultPrefsValues()
        {
            var defaultValues = new Dictionary<string, object>
            {
                { "jukebox.volumeBoost", 0f },
                { "jukebox.individualBoostPerTrack", false },
                { "jukebox.showTrackPanelIndefinitely", false },
                { "jukebox.alwaysPlayIntro", true },
                { "jukebox.resumeLastSong", false },
                { "jukebox.discordAndSteamIntegration", true },
                { "jukebox.enableTracksPreview", true },
                { "jukebox.preventDuplicateTracks", false },
                { "jukebox.nowPlayingHud", true },
                { "jukebox.enableCalmThemes", true },
                { "jukebox.songPanelStyle", 0 },
                { "jukebox.currentPlaylist", "slot1" },
                { "jukebox.calmThemeEnemiesThreshold", 2 },
                { "jukebox.effects.parry", null },
                { "jukebox.effects.wavecomplete", null },
                { "jukebox.effects.gameover", null },
                { "jukebox.effects.aww", null },
                { "jukebox.effects.terminal-music", null },
                { "jukebox.downloader.separateFolderPerPlaylist", true },
            };
            
            foreach (var value in defaultValues)
                PrefsManager.Instance.defaultValues.Add(value.Key, value.Value);
        }

        private static IEnumerator MotD()
        {
            yield return new WaitForSeconds(1.5f);
            var menu = JukeboxInputs.Instance.Menu;
            var bindings = menu.bindings.Join(binding => menu.GetBindingDisplayStringWithoutOverride(binding));
            HudMessageReceiver.Instance.SendHudMessage($"CGME menu is available by pressing <color=orange>{bindings}</color>");
            _motDShown = true;
        }
    }
}