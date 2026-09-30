using Jukebox.Components.HUD;
using ThornClient.HUD;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Jukebox.ThornExtensions.Modules
{
    public sealed class NowPlayingModule() : FramedHudModule(
        "dev.flazhik.jukebox.nowPlaying",
        "Now Playing",
        "Shows the currently playing track")
    {
        private static GameObject _nowPlayingHudPrefab;
        private static Sprite _icon;

        public NowPlayingThornHud hud;
        public override Sprite Icon => _icon;
        public override string[] Tags => ["music", "cgme", "player", "nowplaying"];
        public override bool HideBackgroundWhenEmpty => true;

        public static void Load()
        {
            _nowPlayingHudPrefab = Addressables
                .LoadAssetAsync<GameObject>("Assets/Jukebox/UI/NowPlayingHudThorn.prefab").WaitForCompletion();
            _icon = Addressables.LoadAssetAsync<Sprite>("Assets/Jukebox/Textures/ThornIcons/note.png")
                .WaitForCompletion();
        }

        protected override GameObject CreateContentObject()
        {
            var go = Object.Instantiate(_nowPlayingHudPrefab);
            hud = go.GetComponent<NowPlayingThornHud>();
            return go;
        }
    }
}