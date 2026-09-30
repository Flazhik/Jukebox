using Jukebox.UI.Subtitles;
using ThornClient.HUD;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Jukebox.ThornExtensions.Modules
{
    public sealed class SubtitlesModule() : FramedHudModule(
        "dev.flazhik.jukebox.subtitles",
        "Subtitles",
        "Displays subtitles for Cybergrind songs")
    {
        private static GameObject _subtitlesPrefab;
        private static Sprite _icon;

        public SubtitlesPanel hud;
        public override Sprite Icon => _icon;
        public override string[] Tags => ["music", "cgme", "player", "subtitles", "subs"];
        public override bool HideBackgroundWhenEmpty => true;

        public static void Load()
        {
            _subtitlesPrefab = Addressables
                .LoadAssetAsync<GameObject>("Assets/Jukebox/UI/SubtitlesPanelThorn.prefab").WaitForCompletion();
            _icon = Addressables.LoadAssetAsync<Sprite>("Assets/Jukebox/Textures/ThornIcons/subtitles.png")
                .WaitForCompletion();
        }

        protected override GameObject CreateContentObject()
        {
            var go = Object.Instantiate(_subtitlesPrefab);
            hud = go.GetComponent<SubtitlesPanel>();
            return go;
        }
    }
}