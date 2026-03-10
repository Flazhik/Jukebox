using System;
using System.Collections.Generic;
using System.Linq;
using Jukebox.UI.Elements;
using Jukebox.Utils;
using JukeboxCore.Components;
using JukeboxCore.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Playlist;
namespace Jukebox.Components
{
    public class JukeboxPlayback: DirectoryTreeBrowser<SongIdentifier>
    {
        [SerializeField]
        private Sprite defaultIcon;

        [SerializeField]
        private GameObject disclaimer;
        
        [SerializeField]
        private GameObject buttonsSection;
        
        [SerializeField]
        private RewindSlider rewindSlider;
        
        [SerializeField]
        private TMP_Text currentTimestamp;
        
        [SerializeField]
        private TMP_Text totalLength;
        
        [NonSerialized]
        private JukeboxMusicPlayer player;

        protected override int maxPageLength => 5;
        protected override IDirectoryTree<SongIdentifier> baseDirectory =>
            new FakeDirectoryTree<SongIdentifier>("Playlist", order);
        private CustomContentButton CurrentButton => buttons
            .ElementAtOrDefault(JukeboxMusicPlayer.CurrentSongIndex % maxPageLength)
            ?.GetComponent<CustomContentButton>();

        private readonly List<Transform> buttons = new();
        private IEnumerable<SongIdentifier> order = new List<SongIdentifier>();

        protected void Awake()
        {
            player = JukeboxManager.Instance.player;
            JukeboxMusicPlayer.OnOrderChange += OnOrderChange;
            JukeboxMusicPlayer.OnNextAudioClip += NextAudioClip;
            JukeboxMusicPlayer.OnStop += OnStop;

            rewindSlider.OnValueChanged += position =>
            {
                player.RewindTo(position);
                rewindSlider.slider.value = position;
            };
        }

        protected void OnDestroy()
        {
            JukeboxMusicPlayer.OnStop -= OnStop;
            JukeboxMusicPlayer.OnOrderChange -= OnOrderChange;
            JukeboxMusicPlayer.OnNextAudioClip -= NextAudioClip;
        }

        protected void OnEnable() => GoToBase();
        
        public new void NextPage() => SetPage(currentPage == maxPages - 1 ? 0 : currentPage + 1);

        public new void PreviousPage() => SetPage(currentPage == 0 ? maxPages - 1 : currentPage - 1);

        private void Update()
        {
            if (player.Source == null || player.Source.clip == null)
                return;

            if (!rewindSlider.beingDragged)
                rewindSlider.slider.value = player.Source.time;
            
            currentTimestamp.text = player.Source.time.SecondsToHumanReadable();
        }

        protected override Action BuildLeaf(SongIdentifier id, int indexInPage)
        {
            var metadata = JukeboxSongsLoader.Instance.Load(id).Metadata;
            var go = Instantiate(itemButtonTemplate, itemButtonTemplate.transform.parent);
            var contentButton = go.GetComponent<CustomContentButton>();
            var artist = !string.IsNullOrEmpty(metadata.Artist)
                ? $"<color=#gray>{metadata.Artist}</color>"
                : string.Empty;
            
            contentButton.text.text = $"{metadata.Title} {artist}";
            contentButton.icon.sprite = metadata.Icon != null ? metadata.Icon : defaultIcon;
            go.SetActive(true);
            buttons.Add(go.transform);

            if (PageOf(JukeboxMusicPlayer.CurrentSongIndex) == currentPage && contentButton == CurrentButton)
            {
                contentButton.border.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                Destroy(go.GetComponent<Button>());
            }
            else
            {
                contentButton.button.onClick.AddListener(() =>
                    JukeboxManager.Instance.player.ChangeTrack(currentPage * maxPageLength + indexInPage - 1));
            }

            return () => Destroy(go);
        }

        private void OnOrderChange(IEnumerable<SongIdentifier> songs)
        {
            order = songs;
            Rebuild();
        }
        
        private void NextAudioClip(ClipChangedArgs clip)
        {
            var audioClip = player.Source.clip;
            rewindSlider.slider.value = 0;
            rewindSlider.slider.maxValue = audioClip.length;
            totalLength.text = audioClip.length.SecondsToHumanReadable();
            
            buttonsSection.SetActive(true);
            rewindSlider.slider.interactable = true;

            disclaimer.SetActive(false);
            Rebuild(false);
        }

        private void OnStop()
        {
            rewindSlider.slider.interactable = false;
        }

        public new void GoToBase()
        {
            if (Equals(currentDirectory, baseDirectory))
                return;
            currentDirectory = baseDirectory;
            Rebuild(false);
        }

        public override void Rebuild(bool setToPageZero = true)
        {
            buttons.Clear();
            LayoutRebuilder.ForceRebuildLayoutImmediate(itemParent as RectTransform);
            base.Rebuild(setToPageZero);
        }
    }
}