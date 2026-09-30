using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jukebox.UI;
using Jukebox.Utils;
using JukeboxCore.Components;
using JukeboxCore.Songs.Models;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using static Playlist.SongIdentifier;
using static JukeboxCore.Logging.JukeboxLogger;
using Object = UnityEngine.Object;

namespace Jukebox.Components;

public class JukeboxPlaylistEditor : DirectoryTreeBrowser<JukeboxSong>
{
    [SerializeField] private JukeboxSongsLoader loader;
    [SerializeField] private JukeboxSoundtrackBrowser browser;
    [SerializeField] private Sprite defaultIcon;
    [SerializeField] private Sprite loopSprite;
    [SerializeField] private Sprite loopOnceSprite;

    [Header("UI Elements")] [SerializeField]
    private Image loopModeImage;
    [SerializeField] private Image shuffleImage;
    [SerializeField] private RectTransform selectedControls;
    [SerializeField] private List<Transform> anchors;

    public JukeboxPlaylist playlist = new();

    private readonly List<Transform> buttons = [];
    private readonly Dictionary<Transform, Coroutine> changeAnchorRoutines = new();

    protected override int maxPageLength => anchors.Count;

    protected override IDirectoryTree<JukeboxSong> baseDirectory =>
        new FakeDirectoryTree<JukeboxSong>("Songs", playlist.ids.Select(id => loader.Get(id)));

    private JukeboxCustomContentButton CurrentButton =>
        buttons.ElementAtOrDefault(playlist.selected % maxPageLength)?.GetComponent<JukeboxCustomContentButton>();

    private void Start()
    {
        ReloadPlaylist();
        PrefsManager.onPrefChanged += OnPrefChange;
    }

    private void OnDestroy()
    {
        playlist.OnChanged -= SavePlaylist;
        PrefsManager.onPrefChanged -= OnPrefChange;
    }

    public void ReloadPlaylist()
    {
        try
        {
            LoadPlaylist();
        }
        catch (JsonReaderException ex)
        {
            Log.LogError("Error loading Playlist.json: '" + ex.Message + "'. Recreating file.");
            File.Delete(JukeboxPlaylist.CurrentPath);
            LoadPlaylist();
        }

        Select(playlist.selected);
        SetLoopMode(playlist.loopMode);
        SetShuffle(playlist.shuffled);
        playlist.OnChanged += SavePlaylist;
    }

    public void SavePlaylist()
    {
        File.WriteAllText(JukeboxPlaylist.CurrentPath, JsonConvert.SerializeObject(playlist));
    }

    public void LoadPlaylist()
    {
        Log.LogInfo($"Loading Playlist {JukeboxPlaylist.CurrentPath}");
        JukeboxPlaylist deserialized;
        using (var streamReader = new StreamReader(File.Open(JukeboxPlaylist.CurrentPath, FileMode.OpenOrCreate)))
        {
            deserialized = JsonConvert.DeserializeObject<JukeboxPlaylist>(streamReader.ReadToEnd());
        }

        if (deserialized == null)
            CreateEmpty();
        else
        {
            deserialized.RemoveUnplayable();
            if (deserialized.ids.Count == 0)
                CreateEmpty();
            else
                playlist = deserialized;
            currentDirectory = baseDirectory;
            Rebuild();
        }

        if (PrefsManager.Instance!.GetBoolLocal("jukebox.preventDuplicateTracks"))
            playlist.RemoveDuplicates();

        Rebuild();
        return;

        void CreateEmpty()
        {
            Log.LogInfo("No saved playlist found at " + JukeboxPlaylist.CurrentPath + ". Creating default...");
            CreateDefaultPlaylist();
        }
    }

    public void Remove()
    {
        playlist.Remove(playlist.selected);
        if (playlist.selected >= playlist.ids.Count)
            Select(playlist.Count - 1);
        Rebuild(false);
    }

    public void ClearPlaylist()
    {
        playlist.ids.Clear();
        var meganekoMyBeloved = browser.rootFolder[0];
        playlist.Add(new Playlist.SongIdentifier(meganekoMyBeloved.AssetGUID, IdentifierType.Addressable));
        Rebuild();
    }

    public new void NextPage() => SetPage(currentPage == maxPages - 1 ? 0 : currentPage + 1);
    public new void PreviousPage() => SetPage(currentPage == 0 ? maxPages - 1 : currentPage - 1);
    public void MoveUp() => Move(-1);
    public void MoveDown() => Move(1);

    public void Move(int amount)
    {
        var index1 = playlist.selected % maxPageLength;
        var index2 = index1 + amount;
        var songsCount = playlist.ids.Count;
        var flag = PageOf(playlist.selected) == PageOf((playlist.selected + amount).Mod(songsCount));

        playlist.Swap(playlist.selected, (playlist.selected + amount).Mod(songsCount));
        if (flag)
        {
            ChangeAnchorOf(buttons[index1], anchors[index2]);
            ChangeAnchorOf(selectedControls, anchors[index2]);
            ChangeAnchorOf(buttons[index2], anchors[index1]);
            var button = CurrentButton;
            buttons.RemoveAt(index1);
            buttons.Insert(index2, button.transform);
            Select(playlist.selected + amount, false);
        }
        else
        {
            selectedControls.gameObject.SetActive(false);
            Select((playlist.selected + amount).Mod(songsCount));
        }
    }

    public void ChangeAnchorOf(Transform obj, Transform anchor, float time = 0.15f)
    {
        if (changeAnchorRoutines.ContainsKey(obj))
        {
            if (changeAnchorRoutines[obj] != null)
                StopCoroutine(changeAnchorRoutines[obj]);
            changeAnchorRoutines.Remove(obj);
        }

        changeAnchorRoutines.Add(obj, StartCoroutine(ChangeAnchorOverTime()));

        IEnumerator ChangeAnchorOverTime()
        {
            var t = 0.0f;
            while (t < (double)time && time > 0.0)
            {
                obj.position = Vector3.MoveTowards(obj.position, anchor.position, Time.deltaTime * 2f);
                if (Vector3.Distance(obj.position, anchor.position) > (double)Mathf.Epsilon)
                    yield return null;
                else
                    break;
            }

            obj.position = anchor.position;
        }
    }

    public void ToggleLoopMode() => SetLoopMode(playlist.loopMode == Playlist.LoopMode.Loop ? Playlist.LoopMode.LoopOne : Playlist.LoopMode.Loop);

    private void SetLoopMode(Playlist.LoopMode mode)
    {
        playlist.loopMode = mode;
        loopModeImage.sprite = playlist.loopMode == Playlist.LoopMode.Loop ? loopSprite : loopOnceSprite;
    }

    public void ToggleShuffle() => SetShuffle(!playlist.shuffled);

    private void SetShuffle(bool shuffle)
    {
        playlist.shuffled = shuffle;
        shuffleImage.color = shuffle ? Color.white : Color.gray;
    }

    public void Select(int newIndex, bool rebuild = true)
    {
        if (newIndex < 0 || newIndex >= playlist.Count)
        {
            Log.LogWarning("Attempted to set current index outside bounds of playlist");
            return;
        }

        var num = PageOf(newIndex) == currentPage ? 1 : 0;
        if ((bool)(Object)CurrentButton)
        {
            CurrentButton.border.color = Color.white;
            if (CurrentButton.iconInset != null)
                CurrentButton.iconInset.color = Color.white;
        }

        var selected = playlist.selected;
        playlist.selected = newIndex;
        if (PageOf(selected) < PageOf(newIndex))
            ChangeAnchorOf(selectedControls, anchors.First(), 0.0f);
        else if (PageOf(selected) > PageOf(newIndex))
            ChangeAnchorOf(selectedControls, anchors.Last(), 0.0f);
        if ((bool)(Object)CurrentButton)
        {
            CurrentButton.border.color = Color.red;
            if (CurrentButton.iconInset != null)
                CurrentButton.iconInset.color = Color.red;
        }

        var anchor = anchors[playlist.selected % maxPageLength];
        if (num != 0)
        {
            selectedControls.gameObject.SetActive(true);
            ChangeAnchorOf(selectedControls, anchor);
        }
        else
        {
            selectedControls.gameObject.SetActive(false);
            selectedControls.transform.position = anchor.position;
        }

        if (!rebuild)
            return;
        Rebuild(false);
    }

    public override void Rebuild(bool setToPageZero = true)
    {
        foreach (var changeAnchorRoutine in changeAnchorRoutines)
            if (changeAnchorRoutine.Value != null)
                StopCoroutine(changeAnchorRoutine.Value);
        changeAnchorRoutines.Clear();
        buttons.Clear();
        base.Rebuild(setToPageZero);
        if (buttons.Count < maxPageLength)
            ChangeAnchorOf(plusButton.transform, anchors[buttons.Count], 0.0f);
        LayoutRebuilder.ForceRebuildLayoutImmediate(itemParent as RectTransform);
    }

    protected override Action BuildLeaf(JukeboxSong song, int currentIndex)
    {
        var metadata = song.Metadata;
        var go = Instantiate(itemButtonTemplate, itemButtonTemplate.transform.parent);
        var contentButton = go.GetComponent<JukeboxCustomContentButton>();
        var artist = !string.IsNullOrEmpty(metadata.Artist)
            ? $"<color=#gray>{metadata.Artist}</color>"
            : string.Empty;

        var properties = metadata.GetCompositeProperties();

        contentButton.text.text = $"{metadata.Title} {artist}";
        contentButton.icon.sprite = metadata.Icon != null ? metadata.Icon : defaultIcon;
        contentButton.introAndLoopIcon.SetActive(properties.GotIntroAndLoop);
        contentButton.calmThemeIcon.SetActive(properties.GotCalmTheme
                                              || properties.GotCalmIntro);
        go.SetActive(true);
        ChangeAnchorOf(go.transform, anchors[currentIndex], 0.0f);
        buttons.Add(go.transform);
        if (PageOf(playlist.selected) == currentPage && contentButton == CurrentButton)
        {
            contentButton.border.color = Color.red;
            if (CurrentButton.iconInset != null)
                CurrentButton.iconInset.color = Color.red;
            selectedControls.gameObject.SetActive(true);
            ChangeAnchorOf(selectedControls, anchors[currentIndex]);
            return () =>
            {
                selectedControls.gameObject.SetActive(false);
                Destroy(go);
            };
        }

        contentButton.button.onClick.AddListener(() =>
        {
            Select(buttons.IndexOf(contentButton.transform) + currentPage * maxPageLength);
            if (!PrefsManager.Instance.GetBoolLocal("jukebox.enableTracksPreview") || !FistControl.Instance.shopping)
                return;

            FindObjectOfType<PreviewHelper>().PlayPreview(loader.Get(song.Id));
        });
        return () => Destroy(go);
    }

    private void CreateDefaultPlaylist()
    {
        foreach (var assetReference in browser.rootFolder)
            playlist.Add(new Playlist.SongIdentifier(assetReference.AssetGUID, IdentifierType.Addressable));
    }

    private void OnPrefChange(string key, object value)
    {
        if (!key.Equals("jukebox.preventDuplicateTracks"))
            return;

        if (!(bool)value)
            return;

        playlist.RemoveDuplicates();
        Rebuild();
    }
}