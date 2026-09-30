using Jukebox.Components.HUD;
using ThornClient.HUD;
using ThornClient.Managers;
using ThornClient.System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Jukebox.ThornExtensions.Modules
{
    public sealed class PlaybackTimeModule() : FramedHudModule(
        "dev.flazhik.jukebox.playbackTime",
        "Playback Time",
        "Displays the elapsed playback time and track duration")
    {
        private static GameObject _nowPlayingHudPrefab;

        public PlaybackTimeThornHud hud;
        public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "clock");
        public override string[] Tags => ["music", "cgme", "player", "nowplaying", "time", "playback"];
        public override bool HideBackgroundWhenEmpty => true;

        public static void Load() =>
            _nowPlayingHudPrefab = Addressables
                .LoadAssetAsync<GameObject>("Assets/Jukebox/UI/PlaybackTimeHudThorn.prefab").WaitForCompletion();

        protected override GameObject CreateContentObject()
        {
            var go = Object.Instantiate(_nowPlayingHudPrefab);
            hud = go.GetComponent<PlaybackTimeThornHud>();
            return go;
        }
    }
}