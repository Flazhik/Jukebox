using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using JukeboxCore;
using JukeboxCore.Exceptions;
using JukeboxCore.Models.Downloader;
using JukeboxDownloader.Service;
using JukeboxDownloader.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static JukeboxCore.Utils.PathsUtils;

namespace JukeboxDownloader.Components
{
    public class JukeboxDownloader : MonoBehaviour
    {
        private const float OnUrlChangedDelay = 1f;

        [SerializeField] public JukeboxDownloaderBrowser browser;
        [SerializeField] public DownloaderErrorDialogue errorDialogue;
        [SerializeField] public TMP_InputField urlInputField;
        [SerializeField] public GameObject downloadProgress;
        [SerializeField] public GameObject loadingPlaceholder;
        [SerializeField] public GameObject bottomPanel;
        [SerializeField] public GameObject cookiesRequiredWindow;
        [SerializeField] public Button downloadAllButton;
        
        private JukeboxDownloaderService downloaderService;
        private MainThreadDispatcher mainThread;
        private Coroutine urlChangedRoutine;
        private TotalDownloadProgress downloadProgressComponent;

        private void Awake()
        {
            downloaderService = JukeboxDownloaderService.Instance;
            mainThread = MainThreadDispatcher.Instance;
            downloadProgressComponent = downloadProgress.GetComponent<TotalDownloadProgress>();
        }

        private void Update()
        {
            var noProgress = downloaderService.TotalEnqueued == 0
                             && downloaderService.TotalDownloaded == 0
                             && downloaderService.TotalFailed == 0;
            
            downloadProgress.SetActive(!noProgress);
            if (noProgress)
                return;

            downloadProgressComponent.Downloaded = downloaderService.TotalDownloaded;
            downloadProgressComponent.Failed = downloaderService.TotalFailed;
            downloadProgressComponent.Enqueued = downloaderService.TotalEnqueued;
        }

        private void OnEnable()
        {
            urlInputField.onValueChanged.AddListener(OnURLChanged);
            
            ReloadBrowserMetadata(downloaderService.Metadata);
            OnDownloaderState(downloaderService.State);
            downloaderService.OnMetadataChanged += ReloadBrowserMetadata;
            downloaderService.OnStateChanged += OnDownloaderState;
        }

        private void OnDisable()
        {
            urlInputField.onValueChanged.RemoveListener(OnURLChanged);
            downloaderService.OnMetadataChanged -= ReloadBrowserMetadata;
            downloaderService.OnStateChanged -= OnDownloaderState;
        }

        public void Cancel() => downloaderService.CancelEverything();

        private void OnURLChanged(string value)
        {
            if (urlChangedRoutine != default)
                StopCoroutine(urlChangedRoutine);

            urlChangedRoutine = StartCoroutine(URLChangedRoutine(value));
        }

        private void ReloadBrowserMetadata(List<DownloadableSongMetadata> metadata)
        {
            var metadataExists = metadata?.Count != 0;
            bottomPanel.SetActive(metadataExists);
            downloadAllButton.gameObject.SetActive(metadataExists);
            browser.ReloadMetadata(metadata);
        }
        
        public void DownloadAll()
        {
            foreach (var track in browser.Metadata)
                downloaderService.EnqueueDownload(track.Id, track.Url, track.Playlist);
        }

        private void OnDownloaderState(JukeboxDownloaderState state)
        {
            switch (state)
            {
                case JukeboxDownloaderState.Idle:
                {
                    loadingPlaceholder.SetActive(false);
                    break;
                }
                
                case JukeboxDownloaderState.LoadingMetadata:
                {
                    browser.Clear();
                    bottomPanel.SetActive(false);
                    loadingPlaceholder.SetActive(true);
                    break;
                }
            }
        }

        private IEnumerator URLChangedRoutine(string value)
        {
            yield return new WaitForSecondsRealtime(OnUrlChangedDelay);

            errorDialogue.gameObject.SetActive(false);
            if (!JukeboxDownloaderService.SupportsUrl(value))
                yield break;
            
            downloaderService.CancelEverything();
            downloaderService.Clear();
            downloadProgress.SetActive(false);
            
            Run(() => downloaderService.LoadMetadata(value));
        }

        private void Run(Func<Task> function) => Task.Run(function).ContinueWith(ErrorHandler());

        private Action<Task> ErrorHandler() => t =>
        {
            mainThread.Enqueue(() =>
            {
                if (t.IsFaulted)
                    RaiseErrorDialogue(t.Exception);
            });
        };
        
        private void RaiseErrorDialogue(Exception e)
        {
            if (e is AggregateException)
            {
                switch (e.InnerException)
                {
                    case CookiesRequiredException:
                    case InvalidCookiesFileException:
                        cookiesRequiredWindow.SetActive(true);
                        break;
                    case JukeboxException jukeboxException:
                        errorDialogue.gameObject.SetActive(true);
                        errorDialogue.messageText.text = jukeboxException.Message;
                        break;
                }
            }
            else
                errorDialogue.messageText.text = $"Unexpected error {e.Message}";
        }
        
        public void RevealModLocation() => Application.OpenURL(AssemblyPath);
        
        public void OpenURL(string url) => Application.OpenURL(url);
    }
}