using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using BepInEx;
using HarmonyLib;
using Jukebox.Components;
using Jukebox.Input;
using JukeboxCore.Assets;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using static JukeboxCore.Utils.PathsUtils;

namespace Jukebox
{
    [BepInProcess("ULTRAKILL.exe")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]
    public class JukeboxPlugin : BaseUnityPlugin
    {
        [ExternalAsset("Assets/Jukebox/Bootstrap.prefab", typeof(GameObject))]
        private static GameObject _bootstrap;

        private static readonly string CatalogDir;
        private static Harmony _harmony;
        
        private static bool _motDShown;
        private static bool _init;

        static JukeboxPlugin()
        {
            CatalogDir = Path.Combine(AssemblyPath!, "Assets");
        }

        private void Awake()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            
            Addressables.InitializeAsync().WaitForCompletion();
            Addressables.LoadContentCatalogAsync(Path.Combine(CatalogDir, "catalog.json"), true).WaitForCompletion();

            _harmony = new Harmony(PluginInfo.GUID);
            Startup();
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

                    AssetsManager.Instance.LoadAssets(Resources.Jukebox);
                    AssetsManager.Instance.RegisterPrefabs(Assembly.GetExecutingAssembly());
                    _init = true;
                    break;
                }
                case "Endless":
                {
                    var o = Instantiate(_bootstrap);
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
            HudMessageReceiver.Instance.SendHudMessage($"Jukebox menu is available by pressing <color=orange>{bindings}</color>");
            _motDShown = true;
        }
    }
}