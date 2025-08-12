using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jukebox.Components;
using JukeboxCore.Components;
using JukeboxCore.Models.Song;
using UnityEngine;
using UnityEngine.UI;
using static Newtonsoft.Json.JsonConvert;
using static Playlist;
namespace Jukebox.UI.SongPanel
{
    public class PlaylistSelectable: DirectoryTreeBrowser<JukeboxSong>
    {
        [SerializeField]
        public GameObject selectPlaylistPanel;
        
        [SerializeField]
        public string playlistName;

        [SerializeField]
        public Button button;
        
        [SerializeField]
        public GameObject empty;
        
        protected override int maxPageLength => 6;
        
        protected override IDirectoryTree<JukeboxSong> baseDirectory
        {
            get
            {
                JukeboxPlaylist playlist;
                using (var streamReader = new StreamReader(File.Open(JukeboxPlaylist.PathForPlaylist(playlistName), FileMode.OpenOrCreate)))
                    playlist = DeserializeObject<JukeboxPlaylist>(streamReader.ReadToEnd());

                var ids = playlist != null ? playlist.ids : new List<SongIdentifier>();
                return new FakeDirectoryTree<JukeboxSong>("Songs", ids
                    .Where(id => id.type != SongIdentifier.IdentifierType.File || File.Exists(id.path))
                    .Select(id => JukeboxSongsLoader.Instance.Load(id)));
            }
        }

        private void Awake()
        {
            button.onClick.AddListener(() =>
            {
                PrefsManager.Instance.SetStringLocal("jukebox.currentPlaylist", playlistName);
                JukeboxManager.Instance.playlistEditor.ReloadPlaylist();
                selectPlaylistPanel.SetActive(false);
            });
        }

        private void OnEnable()
        {
            currentDirectory = baseDirectory;
            Rebuild();
        }

        protected override Action BuildLeaf(JukeboxSong item, int indexInPage)
        {
            var go = Instantiate(itemButtonTemplate, itemButtonTemplate.transform.parent);
            go.SetActive(true);
            go.GetComponent<Image>().sprite = item.Metadata.Icon;
            return () => Destroy(go);
        }
        
        public override void Rebuild(bool setToPageZero = true)
        {
            base.Rebuild(setToPageZero);
            LayoutRebuilder.ForceRebuildLayoutImmediate(itemParent as RectTransform);
            empty.SetActive(!currentDirectory.files.Any());
        }
    }
}