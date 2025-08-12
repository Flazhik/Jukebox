using Jukebox.Components;
using JukeboxCore.Models.Song;
using SettingsMenu.Components;
using UnityEngine;
using UnityEngine.UI;
using static Jukebox.Utils.ReflectionUtils;
using Settings = SettingsMenu.Components.SettingsMenu;

namespace Jukebox.UI.Windows
{
    public class JukeboxMenuWindow : JukeboxWindow
    {
        [SerializeField]
        protected Button[] buttons;

        [SerializeField]
        protected GameObject activeBgObject;

        [SerializeField]
        protected GameObject generalSettings;
        
        [SerializeField]
        protected Button downloaderButton;
        
        [SerializeField]
        protected GameObject downloaderPage;

        public override string StateKey() => "jukeboxMenu";

        protected override void Awake()
        {
            base.Awake();
            foreach (var button in buttons)
                button.onClick.AddListener(() => SetActiveButton(button));

            JukeboxMusicPlayer.OnNextSong += OnNextSong;
        }
        
        public void OnNextSong(JukeboxSong song)
        {
            if (!song.IsCustom)
                return;
            
            var settingsSlider = generalSettings.GetComponentInChildren<SettingsSlider>(true);
            var slider = GetPrivate<Slider>(settingsSlider, typeof(SettingsSlider), "slider");
            slider.value = JukeboxManager.Instance.player.VolumeBoost;
        }

        public void OpenDownloader()
        {
            var settings = gameObject.GetComponent<Settings>();
            if (!GetPrivate<bool>(settings, typeof(Settings), "initialized"))
                settings.Initialize();
            settings.SetActivePage(downloaderPage);
            SetActiveButton(downloaderButton);
        }

        protected void OnDestroy()
        {
            JukeboxMusicPlayer.OnNextSong -= OnNextSong;
        }

        private void SetActiveButton(Button button)
        {
            activeBgObject.transform.SetParent(button.transform);
            activeBgObject.transform.SetSiblingIndex(0);
            activeBgObject.transform.localPosition = Vector3.zero;
        } 
    }
}