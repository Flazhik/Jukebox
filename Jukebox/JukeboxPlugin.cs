using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using Jukebox.Assets;
using Jukebox.Input;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Jukebox
{
    [BepInProcess("ULTRAKILL.exe")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]
    public class JukeboxPlugin : BaseUnityPlugin
    {
        [ExternalAsset("Assets/Jukebox/Bootstrap.prefab", typeof(GameObject))]
        private static GameObject bootstrap;
        
        private static Harmony _harmony;
        private static bool motdShown;

        private void Awake()
        {
            AssetsManager.Instance.LoadAssets();
            AssetsManager.Instance.RegisterPrefabs();

            _harmony = new Harmony(PluginInfo.GUID);
            Startup();
        }
        
        private void Startup()
        {
            _harmony.PatchAll();
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
                case "Endless":
                {
                    Instantiate(bootstrap);
                    if (!motdShown)
                        StartCoroutine(Motd());
                    break;
                }
            }
        }

        private static void SetDefaultPrefsValues()
        {
            var defaultValues = new Dictionary<string, object>
            {
                { "jukebox.volumeBoost", 0f },
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
            };
            
            foreach (var value in defaultValues)
                PrefsManager.Instance.defaultValues.Add(value.Key, value.Value);
        }

        private IEnumerator Motd()
        {
            yield return new WaitForSeconds(1.5f);
            var menu = JukeboxInputs.Instance.Menu;
            var bindings = menu.bindings.Join(binding => menu.GetBindingDisplayStringWithoutOverride(binding));
            HudMessageReceiver.Instance.SendHudMessage($"Jukebox menu is available by pressing <color=orange>{bindings}</color>");
            motdShown = true;
        }
    }
}