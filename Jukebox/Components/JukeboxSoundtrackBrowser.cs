using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Jukebox.UI;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Jukebox.Components
{
    public class JukeboxSoundtrackBrowser: DirectoryTreeBrowser<AssetReferenceSoundtrackSong>
    {
        [Header("References")]
        [SerializeField]
        private JukeboxPlaylistEditor playlistEditorLogic;

        [SerializeField]
        private GameObject playlistEditorPanel;

        [SerializeField]
        private JukeboxTerminalPanel navigator;
        
        [SerializeField]
        private GameObject loadingPrefab;
        
        [SerializeField]
        private Sprite lockedLevelSprite;
        
        [SerializeField]
        private Sprite defaultIcon;
        
        [Header("Load all warning")]
        [SerializeField]
        private TMP_Text loadAllWarningText;
        
        [SerializeField]
        private GameObject yesAndNoButtons;

        [SerializeField]
        private GameObject imADumbassButton;

        [Header("Assets")]
        private readonly Dictionary<AssetReferenceSoundtrackSong, SoundtrackSong> referenceCache = new();
        public List<AssetReferenceSoundtrackSong> rootFolder = new();
        public List<SoundtrackFolder> levelFolders = new();
        public List<AssetReferenceSoundtrackSong> secretLevelFolder = new();
        public List<AssetReferenceSoundtrackSong> primeFolder = new();
        public List<AssetReferenceSoundtrackSong> encoreFolder = new();
        public List<AssetReferenceSoundtrackSong> miscFolder = new();
        private FakeDirectoryTree<AssetReferenceSoundtrackSong> _baseDirectory;

        protected override int maxPageLength => 4;

        protected override IDirectoryTree<AssetReferenceSoundtrackSong> baseDirectory
        {
            get
            {
                if (_baseDirectory == null)
                {
                    levelFolders.Add(new SoundtrackFolder("Secret Levels", secretLevelFolder));
                    for (var level = 1; level <= 3; ++level)
                    {
                        if (GameProgressSaver.GetPrime(0, level) > 0)
                        {
                            levelFolders.Add(new SoundtrackFolder("Prime Sanctums", primeFolder));
                            break;
                        }
                    }
                    if (GameProgressSaver.GetEncoreProgress(0) > 0)
                        levelFolders.Add(new SoundtrackFolder("Encores", encoreFolder));
                    levelFolders.Add(new SoundtrackFolder("Miscellaneous Tracks", miscFolder));
                    levelFolders.Insert(0, new SoundtrackFolder("The Cyber Grind", rootFolder));
                    _baseDirectory = Folder("OST", children: levelFolders.Select(f => new FakeDirectoryTree<AssetReferenceSoundtrackSong>(f.name, f.songs)).Cast<IDirectoryTree<AssetReferenceSoundtrackSong>>().ToList());
                }
                return _baseDirectory;
            }
        }
        
        private void OnEnable() => Rebuild();
        
        public void LoadAllFromCurrentFolder()
        {
            foreach (var file in currentDirectory.files)
                playlistEditorLogic.playlist.Add(new Playlist.SongIdentifier(file.AssetGUID, Playlist.SongIdentifier.IdentifierType.Addressable));
            playlistEditorLogic.Rebuild();
        }
        
        public new void NextPage() => SetPage(currentPage == maxPages - 1 ? 0 : currentPage + 1);

        public new void PreviousPage() => SetPage(currentPage == 0 ? maxPages - 1 : currentPage - 1);

        public void SetWarningText()
        {
            if (!currentDirectory.files.Any())
            {
                yesAndNoButtons.SetActive(false);
                imADumbassButton.SetActive(true);
                loadAllWarningText.text = "This folder contains no songs.";
            } else {
                loadAllWarningText.text =
                    $"Do you want to add <color=green>{currentDirectory.files.Count()}</color> tracks " +
                    $"from the folder <color=yellow>{currentDirectory.name}</color> to your playlist?";
            }
        }

        private void SelectSong(string id, SoundtrackSong song)
        {
            if (song.clips.Count > 0)
            {
                var count = playlistEditorLogic.playlist.Count;
                var target = playlistEditorLogic.PageOf(count);
                playlistEditorLogic.playlist.Add(new Playlist.SongIdentifier(id, Playlist.SongIdentifier.IdentifierType.Addressable));
                playlistEditorLogic.SetPage(target);
                playlistEditorLogic.Select(count);
                navigator.GoToNoMenu(playlistEditorPanel);
            }
            else
                Debug.LogWarning("Attempted to add song with no clips to playlist.");
        }

        private IEnumerator LoadSongButton(AssetReferenceSoundtrackSong reference, GameObject btn)
        {
            var placeholder = Instantiate(loadingPrefab, itemParent, false);
            placeholder.SetActive(true);
            SoundtrackSong song;
            if (referenceCache.TryGetValue(reference, out var soundtrackSong))
            {
                yield return new WaitUntil(() => referenceCache[reference] != null || btn == null);
                if (btn == null)
                {
                    Destroy(placeholder);
                    yield break;
                }

                song = soundtrackSong;
            }
            else
            {
                var handle = reference.LoadAssetAsync();
                referenceCache.Add(reference, null);
                yield return new WaitUntil(() => handle.IsDone || btn == null);
                if (btn == null)
                {
                    Destroy(placeholder);
                    yield return handle;
                }
                song = handle.Result;
                referenceCache[reference] = song;
                Addressables.Release(handle);
                if (btn == null)
                    yield break;
            }
            Destroy(placeholder);
            var componentInChildren = btn.GetComponentInChildren<JukeboxCustomContentButton>();
            componentInChildren.button.onClick.RemoveAllListeners();
            if (song.conditions.AllMet())
            {
                componentInChildren.icon.sprite = song.icon != null ? song.icon : defaultIcon;
                componentInChildren.text.text = song.songName + " <color=grey>" + song.extraLevelBit + "</color>";
                componentInChildren.costText.text = "Unlocked";
                componentInChildren.button.onClick.AddListener(() => SelectSong(reference.AssetGUID, song));
                componentInChildren.introAndLoopIcon.SetActive(song.introClip != null);
                componentInChildren.calmThemeIcon.SetActive(JukeboxCore.Components.JukeboxStaticData.Instance.calmThemes.FindCalmClipsFor(reference.AssetGUID) != default);
                SetActiveAll(componentInChildren.objectsToActivateIfAvailable, true);
                btn.SetActive(true);
            }
            else
            {
                SetActiveAll(componentInChildren.objectsToActivateIfAvailable, false);
                componentInChildren.text.text = "????????? " + song.extraLevelBit;
                componentInChildren.costText.text = song.conditions.DescribeAll();
                componentInChildren.icon.sprite = lockedLevelSprite;
                componentInChildren.border.color = Color.grey;
                if (componentInChildren.iconInset != null)
                    componentInChildren.iconInset.color = Color.grey;
                componentInChildren.text.color = Color.grey;
                componentInChildren.costText.color = Color.grey;
                btn.SetActive(true);
            }
        }

        protected override Action BuildLeaf(AssetReferenceSoundtrackSong reference, int indexInPage)
        {
            var btn = Instantiate(itemButtonTemplate, itemParent, false);
            StartCoroutine(LoadSongButton(reference, btn));
            return () => Destroy(btn);
        }

        private static void SetActiveAll(List<GameObject> objects, bool active)
        {
            foreach (var gameObject in objects)
                gameObject.SetActive(active);
        }
    }
}