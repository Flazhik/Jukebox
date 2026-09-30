using Jukebox.UI.Subtitles;
using JukeboxCore.Assets;
using ThornClient.HUD;
using UnityEngine;

namespace Jukebox.ThornExtensions.Modules
{
    public class SubtitlesModule() : FramedHudModule(
        "dev.flazhik.jukebox.subtitles",
        "Subtitles",
        "Displays subtitles for Cybergrind songs")
    {
        [AddressableAsset("Assets/Jukebox/UI/SubtitlesPanelThorn.prefab")]
        private static GameObject _subtitlesPrefab;

        public SubtitlesPanel hud;
        public override Sprite Icon => AssetsManager.Load<Sprite>("Assets/Jukebox/Textures/ThornIcons/subtitles.png");
        public override string[] Tags => ["music", "cgme", "player", "subtitles", "subs"];
        public override bool HideBackgroundWhenEmpty => true;

        protected override GameObject CreateContentObject()
        {
            var go = Object.Instantiate(_subtitlesPrefab);
            hud = go.GetComponent<SubtitlesPanel>();
            return go;
        }
    }
}