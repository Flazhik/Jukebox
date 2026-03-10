using System.Threading.Tasks;
using JukeboxCore;
using JukeboxCore.Utils;
using JukeboxDownloader.Models;
using JukeboxDownloader.Service;
using UnityEngine;
using static JukeboxDownloader.Utils.ThirdPartySoftwareUtils;

namespace JukeboxDownloader.Components
{
    public class JukeboxDownloaderContainer : MonoBehaviour
    {
        [SerializeField]
        public GameObject loadingScreen;
        
        [SerializeField]
        public JukeboxDownloader downloader;
        
        [SerializeField]
        public ThirdPartyExecsDownloader thirdPartyExecsDownloader;
        
        [SerializeField]
        public YtDlpUpdater ytDlpUpdater;
        
        private JukeboxDownloaderService downloaderService;

        private void Awake()
        {
            downloaderService = JukeboxDownloaderService.Instance;
            Task.Run(IsYtDlpUpToDate)
                .ContinueWith(ytDlpIsUtToDate => Task.Run(() =>
                {
                    var softwareIntact = AllRequiredSoftwarePresent() && ValidateThirdPartySoftwareIntegrity();

                    MainThreadDispatcher.Instance.Enqueue(() =>
                    {
                        downloaderService.ThirdPartySoftwareState = new ThirdPartyExecsState
                        {
                            state = softwareIntact
                                ? ytDlpIsUtToDate.IsFaulted || ytDlpIsUtToDate.IsCanceled || ytDlpIsUtToDate.Result
                                    ? ExecsState.Present
                                    : ExecsState.UpdateRequired
                                : ExecsState.Missing
                        };
                        loadingScreen.SetActive(false);
                    });
                }));
        }

        private void OnEnable()
        {
            HandleState(downloaderService.ThirdPartySoftwareState);
            downloaderService.OnThirdPartySoftwareChanged += HandleState;
        }

        private void OnDisable()
        {
            downloaderService.OnThirdPartySoftwareChanged -= HandleState;
        }

        private void HandleState(ThirdPartyExecsState state)
        {
            downloader.gameObject.SetActive(state.state == ExecsState.Present);
            if (!state.state.IsOneOf(ExecsState.Downloading, ExecsState.Failed))
            {
                thirdPartyExecsDownloader.gameObject.SetActive(!state.state.IsOneOf(ExecsState.Present, ExecsState.None,
                    ExecsState.UpdateRequired));
                ytDlpUpdater.gameObject.SetActive(state.state == ExecsState.UpdateRequired);
            }
        }
    }
}