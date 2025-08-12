using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JukeboxCore.Models.Downloader;
using JukeboxDownloader.Service;
using JukeboxDownloader.UI;
using UnityEngine;
using UnityEngine.UI;

namespace JukeboxDownloader.Components
{
    public class JukeboxDownloaderBrowser : DirectoryTreeBrowser<DownloadableSongMetadata>
    {
        public List<DownloadableSongMetadata> Metadata { get; private set; } = new();
        
        protected override int maxPageLength => 5;
        protected override IDirectoryTree<DownloadableSongMetadata> baseDirectory =>
            new FakeDirectoryTree<DownloadableSongMetadata>("Downloads", Metadata);
        
        private JukeboxDownloaderService downloaderService;

        protected void Awake()
        {
            downloaderService = JukeboxDownloaderService.Instance;
        }

        public void ReloadMetadata(List<DownloadableSongMetadata> newMetadata)
        {
            Metadata = newMetadata;
            Rebuild();
        }

        public void Clear()
        {
            Metadata.Clear();
            Rebuild();
        }

        protected override Action BuildLeaf(DownloadableSongMetadata item, int indexInPage)
        {
            var go = Instantiate(itemButtonTemplate, itemParent, false);
            var downloadable = go.GetComponent<DownloadableTrackElement>();

            downloadable.id = item.Id;
            downloadable.titleText.text = item.Title;
            downloadable.artistText.text = item.Artist;
            
            downloadable.downloadButton.onClick.AddListener(() => Download());
            downloadable.retryButton.onClick.AddListener(() => Download(true));

            go.SetActive(true);
            Task.Run(() => downloaderService.UIEventManager.DownloadThumbnail(item.Id, item.ThumbnailUrl));
            return () => Destroy(go);

            void Download(bool decrementFailed = false) =>
                downloaderService.EnqueueDownload(item.Id, item.Url, item.Playlist, decrementFailed);
        }
        
        public override void Rebuild(bool setToPageZero = true)
        {
            currentDirectory = baseDirectory;
            base.Rebuild(setToPageZero);
            LayoutRebuilder.ForceRebuildLayoutImmediate(itemParent as RectTransform);
        }
    }
}