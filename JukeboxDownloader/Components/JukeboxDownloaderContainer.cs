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
        
        private JukeboxDownloaderService downloaderService;

        private void Awake()
        {
            downloaderService = JukeboxDownloaderService.Instance;
            Task.Run(() =>
            {
                var softwareIntact = AllRequiredSoftwarePresent() && ValidateThirdPartySoftwareIntegrity();
                    MainThreadDispatcher.Instance.Enqueue(() =>
                    {
                        downloaderService.ThirdPartySoftwareState = new ThirdPartyExecsState
                        {
                            state = softwareIntact
                                ? ExecsState.Present
                                : ExecsState.Missing
                        };
                        loadingScreen.SetActive(false);
                    });
            });
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
            thirdPartyExecsDownloader.gameObject.SetActive(!state.state.IsOneOf(ExecsState.Present, ExecsState.None));
        }
    }
}