using System.Threading.Tasks;
using JukeboxDownloader.Models;
using JukeboxDownloader.Service;
using JukeboxDownloader.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JukeboxDownloader.Components
{
    public class YtDlpUpdater : MonoBehaviour
    {
        [SerializeField]
        public Button updateButton;

        [SerializeField]
        public GradualProgressBar progressBar;
        
        [SerializeField]
        public GameObject errorMessage;

        [SerializeField]
        public TMP_Text progressText;
        
        private JukeboxDownloaderService downloaderService;

        private void Awake()
        {
            downloaderService = JukeboxDownloaderService.Instance;
        }

        private void OnEnable()
        {
            downloaderService.OnThirdPartySoftwareChanged += SetState;
            SetState(downloaderService.ThirdPartySoftwareState);
        }

        private void OnDisable()
        {
            downloaderService.OnThirdPartySoftwareChanged -= SetState;
        }

        private void SetState(ThirdPartyExecsState state)
        {
            HandleState(state.state);
            SetProgressBar(state);
        }

        private void SetProgressBar(ThirdPartyExecsState state, bool forced = false)
        {
            if (!state.progress.HasValue)
            {
                progressBar.ForceSetProgress(0f);
                progressText.text = "Downloading...";
                return;
            }

            if (forced)
                progressBar.ForceSetProgress(state.progress.Value);
            else
                progressBar.UpdateProgress(state.progress.Value);
            
            progressText.text = $"Updating yt-dlp: {(int)(state.progress * 100)}%";
        }

        public void UpdateYtDlp()
        {
            downloaderService.ThirdPartySoftwareState = ThirdPartyExecsState.DownloadingStarted();
            Task.Run(() => downloaderService.UpdateYtDlp());
        }

        private void HandleState(ExecsState state)
        {
            if (state == ExecsState.Present)
            {
                gameObject.SetActive(false);
                return;
            }
            
            updateButton.gameObject.SetActive(state == ExecsState.UpdateRequired);
            progressText.gameObject.SetActive(state == ExecsState.Downloading);
            errorMessage.gameObject.SetActive(state == ExecsState.Failed);
        }
    }
}