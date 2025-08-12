using System.Threading.Tasks;
using JukeboxDownloader.Models;
using JukeboxDownloader.Service;
using JukeboxDownloader.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static JukeboxCore.Utils.PathsUtils;

namespace JukeboxDownloader.Components
{
    public class ThirdPartyExecsDownloader : MonoBehaviour
    {
        [SerializeField]
        public Button installButton;
        
        [SerializeField]
        public GradualProgressBar progressBar;
        
        [SerializeField]
        public GameObject errorMessage;

        [SerializeField]
        public TMP_Text progressText;
        
        private JukeboxDownloaderService downloaderService;
        
        public void RevealModLocation() => Application.OpenURL(AssemblyPath);
        
        public void OpenURL(string url) => Application.OpenURL(url);

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
            var binary = state.currentBinary ?? string.Empty;
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
            
            progressText.text = $"Downloading {binary}: {(int)(state.progress * 100)}%";
        }

        public void Install()
        {
            downloaderService.ThirdPartySoftwareState = ThirdPartyExecsState.DownloadingStarted();
            Task.Run(() => downloaderService.DownloadThirdPartySoftware());
        }

        private void HandleState(ExecsState state)
        {
            if (state == ExecsState.Present)
            {
                gameObject.SetActive(false);
                return;
            }
            
            installButton.gameObject.SetActive(state == ExecsState.Missing);
            progressText.gameObject.SetActive(state == ExecsState.Downloading);
            errorMessage.gameObject.SetActive(state == ExecsState.Failed);
        }
    }
}