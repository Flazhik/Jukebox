using JukeboxCore.Models.Downloader;
using JukeboxDownloader.Service;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static JukeboxCore.Models.Downloader.TrackDownloadingState;

namespace JukeboxDownloader.UI;

public class DownloadableTrackElement : MonoBehaviour
{
    private static readonly Color DefaultColor = new(0f, 0f, 0f, 0.851f);
    private static readonly Color SuccessColor = new(0f, 0.2f, 0f, 0.851f);
    private static readonly Color FailColor = new(0.4f, 0f, 0f, 0.851f);

    [HideInInspector] public string id;

    [SerializeField] public TMP_Text titleText;
    [SerializeField] public TMP_Text artistText;
    [SerializeField] public TMP_Text progressPercentage;
    [SerializeField] public TMP_Text downloadEta;
    [SerializeField] public Image thumbnail;
    [SerializeField] public GradualProgressBar progressBar;
    [SerializeField] public Button downloadButton;
    [SerializeField] public Button retryButton;
    [SerializeField] public GameObject thumbnailPlaceholder;
    [SerializeField] public GameObject downloadProgressSection;
    [SerializeField] public GameObject enqueuedGo;
    [SerializeField] public GameObject preprocessingGo;
    [SerializeField] public GameObject postprocessingGo;

    private DownloaderUIEventManager uiEventManager;

    private void Awake()
    {
        uiEventManager = JukeboxDownloaderService.Instance?.UIEventManager;
    }

    private void OnEnable()
    {
        if (uiEventManager.StateOf(id) is { } state)
            SetTrackState(state);

        uiEventManager.Subscribe(id, SetTrackState);
    }

    private void OnDisable()
    {
        uiEventManager.Unsubscribe(id, SetTrackState);
    }

    private void SetTrackState(DownloadableEntityState state)
    {
        if (state.state.HasValue)
            HandleState(state.state.Value);

        SetThumbnail(state.thumbnail);

        if (state.progress.HasValue)
        {
            progressBar.UpdateProgress(state.progress.Value);
            progressPercentage.text = $"{(int)(state.progress.Value * 100)}%";
        }

        if (state.eta != null)
            downloadEta.text = state.eta;
    }

    private void SetThumbnail(Sprite texture)
    {
        thumbnailPlaceholder.SetActive(texture == null);
        thumbnail.gameObject.SetActive(texture != null);
        thumbnail.sprite = texture;
    }

    private void HandleState(TrackDownloadingState state)
    {
        progressBar.gameObject.SetActive(state == Downloading);
        downloadButton.gameObject.SetActive(state == None);
        retryButton.gameObject.SetActive(state == Failed);
        downloadProgressSection.SetActive(state == Downloading);
        enqueuedGo.SetActive(state == Enqueued);
        preprocessingGo.SetActive(state == PreProcessing);
        postprocessingGo.SetActive(state == PostProcessing);

        GetComponent<Image>().color = state switch
        {
            Success => SuccessColor,
            Failed => FailColor,
            _ => DefaultColor
        };
    }
}
