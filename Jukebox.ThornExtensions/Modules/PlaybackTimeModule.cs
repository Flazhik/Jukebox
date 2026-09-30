using Jukebox.Components.HUD;
using JukeboxCore.Assets;
using ThornClient.HUD;
using ThornClient.Managers;
using ThornClient.System;
using UnityEngine;

namespace Jukebox.ThornExtensions.Modules
{
    public class PlaybackTimeModule() : FramedHudModule(
        "dev.flazhik.jukebox.playbackTime",
        "Playback Time",
        "Displays the elapsed playback time and track duration")
    {
        [AddressableAsset("Assets/Jukebox/UI/PlaybackTimeHudThorn.prefab")]
        private static GameObject _nowPlayingHudPrefab;

        public PlaybackTimeThornHud hud;
        public override Sprite Icon => AssetManager.Get<Sprite>(ClickGUI.BundleKey, "clock");
        public override string[] Tags => ["music", "cgme", "player", "nowplaying", "time", "playback"];
        public override bool HideBackgroundWhenEmpty => true;

        protected override GameObject CreateContentObject()
        {
            var go = Object.Instantiate(_nowPlayingHudPrefab);
            hud = go.GetComponent<PlaybackTimeThornHud>();
            return go;
        }
    }
}