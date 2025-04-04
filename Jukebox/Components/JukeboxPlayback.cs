using System;
using System.Collections.Generic;
using System.Linq;
using Jukebox.Core.Model.Song;
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

        protected override int maxPageLength => 6;
        protected override IDirectoryTree<SongIdentifier> baseDirectory =>
            new FakeDirectoryTree<SongIdentifier>("Playlist", order);
        private CustomContentButton CurrentButton => buttons
            .ElementAtOrDefault(JukeboxMusicPlayer.CurrentSongIndex % maxPageLength)
            ?.GetComponent<CustomContentButton>();

        private readonly List<Transform> buttons = new();
        private IEnumerable<SongIdentifier> order = new List<SongIdentifier>();

        protected void Awake()
        {
            JukeboxMusicPlayer.onOrderChange += OnOrderChange;
            JukeboxMusicPlayer.onNextSong += OnNextSong;
        }

        protected void OnDestroy()
        {
            JukeboxMusicPlayer.onOrderChange -= OnOrderChange;
            JukeboxMusicPlayer.onNextSong -= OnNextSong;
        }

        protected void OnEnable() => GoToBase();
        
        public new void NextPage() => SetPage(currentPage == maxPages - 1 ? 0 : currentPage + 1);

        public new void PreviousPage() => SetPage(currentPage == 0 ? maxPages - 1 : currentPage - 1);

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
        
        private void OnNextSong(JukeboxSong _)
        {
            buttonsSection.SetActive(true);
            disclaimer.SetActive(false);
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