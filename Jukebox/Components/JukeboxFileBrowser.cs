using System;
using System.Linq;
using Jukebox.UI;
using JukeboxCore.Collections;
using JukeboxCore.Models.Song;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static JukeboxCore.Utils.PathsUtils;

namespace Jukebox.Components
{
    public class JukeboxFileBrowser: DirectoryTreeBrowser<JukeboxSong>
    {
        [SerializeField]
        private JukeboxTerminalPanel navigator;

        [SerializeField]
        private JukeboxPlaylistEditor playlistEditorLogic;
        
        [SerializeField]
        private GameObject playlistEditor;

        [Header("Load all warning")]
        [SerializeField]
        private TMP_Text loadAllWarningText;
        
        [SerializeField]
        private GameObject yesAndNoButtons;
        
        [SerializeField]
        private GameObject imADumbassButton;

        [SerializeField]
        private Sprite youtubeIcon;

        protected override int maxPageLength => 4;

        protected override IDirectoryTree<JukeboxSong> baseDirectory => new JukeboxFileTree(MusicPath);
        
        public new void NextPage() => SetPage(currentPage == maxPages - 1 ? 0 : currentPage + 1);

        public new void PreviousPage() => SetPage(currentPage == 0 ? maxPages - 1 : currentPage - 1);

        public void LoadAllFromCurrentFolder()
        {
            foreach (var song in currentDirectory.files)
                playlistEditorLogic.playlist.Add(song.Id);
            playlistEditorLogic.Rebuild();
        }
        
        public void SetWarningText()
        {
            if (!currentDirectory.files.Any())
            {
                yesAndNoButtons.SetActive(false);
                imADumbassButton.SetActive(true);
                loadAllWarningText.text = !currentDirectory.children.Any()
                    ? "Oh, I get it. \"Yes, I would like to add a whopping total of 0 songs\". Very funny."
                    : "This folder contains no songs.";
                return;
            }

            if (currentDirectory.files.Count() == 1)
            {
                loadAllWarningText.text = "It's a single song in there, but ok. Would you like me to add it?";
                return;
            }
            
            loadAllWarningText.text = $"Do you want to add <color=green>{currentDirectory.files.Count()}</color> tracks " +
                                      $"from the folder <color=yellow>{currentDirectory.name}</color> to your playlist?";
        }

        public void Refresh()
        {
            baseDirectory.Refresh();
            Rebuild();
        }
        
        protected override Action BuildLeaf(JukeboxSong song, int indexInPage)
        {
            var go = Instantiate(itemButtonTemplate, itemParent, false);
            var component = go.GetComponent<JukeboxCustomContentButton>();
            var metadata = song.Metadata;
            
            component.button.onClick.AddListener(() =>
            {
                var count = playlistEditorLogic.playlist.Count;
                var target = playlistEditorLogic.PageOf(count);
                playlistEditorLogic.playlist.Add(song.Id);
                playlistEditorLogic.SetPage(target);
                playlistEditorLogic.Select(count);
                navigator.GoToNoMenu(playlistEditor);
            });

            var artist = !string.IsNullOrEmpty(metadata.Artist)
                ? $"<color=#gray>{metadata.Artist}</color>"
                : string.Empty;
            component.text.text = $"{metadata.Title} {artist}";
            component.icon.sprite = metadata.Icon;
            component.introAndLoopIcon.SetActive(metadata.Composite.GotIntroAndLoop);
            component.calmThemeIcon.SetActive(metadata.Composite.GotCalmTheme
                                              || metadata.Composite.GotCalmLoop
                                              || metadata.Composite.GotCalmIntro);
            go.SetActive(true);
            return () => Destroy(go);
        }
        
        protected override Action BuildDirectory(IDirectoryTree<JukeboxSong> folder, int indexInPage)
        {
            var btn = Instantiate(folderButtonTemplate, itemParent, false);
            btn.GetComponent<Button>().onClick.RemoveAllListeners();
            btn.GetComponent<Button>().onClick.AddListener(() => StepDown(folder));
            btn.GetComponentInChildren<TMP_Text>().text = folder.name;
            btn.SetActive(true);

            if (folder.name == "YouTube" && folder.parent?.parent == null)
                btn.transform.Find("Icon").GetComponent<Image>().sprite = youtubeIcon;
            
            return () => Destroy(btn);
        }
    }
}