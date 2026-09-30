using Jukebox.Components.HUD;
using JukeboxCore.Assets;
using ThornClient.HUD;
using UnityEngine;

namespace Jukebox.ThornExtensions.Modules
{
    public class NowPlayingModule() : FramedHudModule(
        "dev.flazhik.jukebox.nowPlaying",
        "Now Playing",
        "Shows the currently playing track")
    {
        [AddressableAsset("Assets/Jukebox/UI/NowPlayingHudThorn.prefab")]
        private static GameObject _nowPlayingHudPrefab;

        public NowPlayingThornHud hud;
        public override Sprite Icon => AssetsManager.Load<Sprite>("Assets/Jukebox/Textures/ThornIcons/note.png");
        public override string[] Tags => ["music", "cgme", "player", "nowplaying"];
        public override bool HideBackgroundWhenEmpty => true;

        protected override GameObject CreateContentObject()
        {
            var go = Object.Instantiate(_nowPlayingHudPrefab);
            hud = go.GetComponent<NowPlayingThornHud>();
            return go;
        }
    }
}