using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Jukebox.Components
{
    public class JukeboxEffectsBrowser : DirectoryTreeBrowser<FileInfo>
    {
        private static readonly DirectoryInfo EffectsDirectory = new(Path.Combine(Directory.GetParent(Application.dataPath)!.FullName,
            "Cybergrind", "Effects"));

        [SerializeField]
        private JukeboxTerminalPanel navigator;
        
        [SerializeField]
        private GameObject effectsMenu;

        private string currentPrefKey;
        
        protected override int maxPageLength => 3;
        
        protected override IDirectoryTree<FileInfo> baseDirectory => new FileDirectoryTree(EffectsDirectory);
        
        public new void NextPage() => SetPage(currentPage == maxPages - 1 ? 0 : currentPage + 1);

        public new void PreviousPage() => SetPage(currentPage == 0 ? maxPages - 1 : currentPage - 1);

        public void SetCurrentPrefKey(string key) => currentPrefKey = key;

        public void ResetToDefault() => PrefsManager.Instance.SetStringLocal(currentPrefKey, null);
        
        protected override Action BuildLeaf(FileInfo item, int indexInPage)
        {
            var go = Instantiate(itemButtonTemplate, itemParent);
            var contentButton = go.GetComponent<CustomContentButton>();
            contentButton.text.text = item.Name;
            go.SetActive(true);

            contentButton.button.onClick.AddListener(() =>
            {
                PrefsManager.Instance.SetStringLocal(currentPrefKey, item.FullName);
                navigator.GoToNoMenu(effectsMenu);
            });
            return () => Destroy(go);
        }

        public void OpenEffectsFolder() =>
            Process.Start(new ProcessStartInfo("explorer", EffectsDirectory.FullName));

        public override void Rebuild(bool setToPageZero = true)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(itemParent as RectTransform);
            base.Rebuild(setToPageZero);
        }
    }
}