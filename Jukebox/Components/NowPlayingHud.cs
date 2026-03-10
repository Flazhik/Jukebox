using System;
using Jukebox.Utils;
using JukeboxCore.Events;
using SettingsMenu.Components.Pages;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Jukebox.Components
{
    public class NowPlayingHud: MonoBehaviour
    {
        private static int HudTypePref => PrefsManager.Instance.GetInt("hudType");
        private static bool EnableHud => PrefsManager.Instance.GetBoolLocal("jukebox.nowPlayingHud");
        
        [SerializeField]
        protected GameObject hud;
        
        [SerializeField]
        protected HudType type;

        [SerializeField]
        protected Image[] background;
        
        [SerializeField]
        protected Image cover;

        [SerializeField]
        protected TMP_Text title;

        [SerializeField]
        protected TMP_Text artist;
        
        [SerializeField]
        protected TMP_Text timestamp;
        
        [SerializeField]
        protected TMP_Text totalDuration;
        
        [SerializeField]
        protected TMP_Text currentClip;

        [SerializeField]
        protected GameObject bar;
        
        public Material normalTextMaterial;
        
        [SerializeField]
        public Material overlayTextMaterial;

        [SerializeField]
        public Material hudMaterial;

        [NonSerialized]
        private JukeboxMusicPlayer player;

        private float progressBarWidth;

        [NonSerialized]
        private float clipDuration;

        protected void OnEnable()
        {
            player = JukeboxManager.Instance.player;
            JukeboxMusicPlayer.OnNextAudioClip += NextAudioClip;
            JukeboxMusicPlayer.OnStop += OnStop;
            PrefsManager.onPrefChanged += OnPrefChanged;

            progressBarWidth = ((RectTransform)bar.transform).rect.width;

            SetOpacity(PrefsManager.Instance.GetFloat("hudBackgroundOpacity"));
            SetAlwaysOnTop(PrefsManager.Instance.GetBool("hudAlwaysOnTop"));
            hud.transform.localPosition = HudPosition();
        }

        protected void OnDisable()
        {
            JukeboxMusicPlayer.OnNextAudioClip -= NextAudioClip;
            JukeboxMusicPlayer.OnStop -= OnStop;
            PrefsManager.onPrefChanged -= OnPrefChanged;
        }

        protected void Update()
        {
            if (player.Source == null || player.Source.clip == null || clipDuration == 0)
                return;

            var elapsed = player.Source.time;

            if (type == HudType.Standard)
                timestamp.text = elapsed.SecondsToHumanReadable();

            var rect = bar.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(progressBarWidth * (-1 + elapsed / clipDuration), rect.sizeDelta.y);
        }

        private void NextAudioClip(ClipChangedArgs clip)
        {
            var hudType = HudTypePref;
            hud.SetActive(EnableHud && (type == HudType.Standard ? hudType == 1 : hudType > 1));

            var song = clip.Song;
            cover.sprite = song.Metadata.Icon;
            title.text = song.Metadata.Title;
            artist.text = song.Metadata.Artist;

            clipDuration = player.Source.clip.length;
            if (type == HudType.Standard)
            {
                totalDuration.text = player.Source.clip.length.SecondsToHumanReadable();
                currentClip.text = JukeboxMusicPlayer.CurrentClipIndex switch
                {
                    -1 => "",
                    0 => "INTRO",
                    _ => song.Clips.Count > 1
                        ? $"{JukeboxMusicPlayer.CurrentClipIndex}/{song.Clips.Count}"
                        : ""
                };
            }
        }

        private void OnStop() => hud.SetActive(false);

        private Vector3 HudPosition()
        {
            var hudPos = hud.transform.localPosition;
            if (type == HudType.Standard)
            {
                var y = 30;

                if (HUDSettings.weaponIconEnabled)
                    y += 400;

                if (PrefsManager.Instance.GetInt("speedometer") > 0)
                    y += 100;
                
                return new Vector3(hudPos.x, y, hudPos.z);
            }

            var x = -147;
            if (!PrefsManager.Instance.GetBool("armIcons"))
                x -= 80;
            
            return new Vector3(x, hudPos.y, hudPos.z);
        }

        private void SetOpacity(float value)
        {
            foreach (var bg in background)
                bg.color = new Color(bg.color.r, bg.color.g, bg.color.b, value / 100);
        }
        
        private void SetAlwaysOnTop(bool onTop)
        {
            foreach (var tmpText in GetComponentsInChildren<TMP_Text>(true))
                tmpText.fontSharedMaterial = onTop ? overlayTextMaterial : normalTextMaterial;

            hudMaterial.SetFloat("_ZTest", onTop ? 8f : 4f);
        }

        private void OnPrefChanged(string key, object value)
        {
            switch (key)
            {
                case "weaponIcons":
                case "speedometer":
                case "armIcons":
                    hud.transform.localPosition = HudPosition();
                    break;
                case "hudBackgroundOpacity":
                    SetOpacity((float) value);
                    break;
                case "hudAlwaysOnTop":
                    SetAlwaysOnTop((bool) value);
                    break;
                case "hudType" when JukeboxMusicPlayer.CurrentSong != null:
                    hud.SetActive(type == HudType.Standard ? (int) value == 1 : (int) value > 1);
                    break;
                case "jukebox.nowPlayingHud" when JukeboxMusicPlayer.CurrentSong != null:
                {
                    hud.SetActive((bool) value);
                    if ((bool)value)
                        NextAudioClip(new ClipChangedArgs(JukeboxMusicPlayer.CurrentSong, true));
                    break;
                }
            }
        }
        
        protected enum HudType
        {
            Standard,
            Classic
        }
    }
}