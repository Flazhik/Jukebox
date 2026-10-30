using TMPro;
using UnityEngine;

namespace JukeboxDownloader.UI;

public class TotalDownloadProgress : MonoBehaviour
{
    [SerializeField] public TMP_Text downloadedText;
    [SerializeField] public TMP_Text failedText;
    [SerializeField] public TMP_Text enqueuedText;

    public int Downloaded
    {
        set
        {
            field = value;
            downloadedText.text = field.ToString();
        }
    }

    public int Failed
    {
        set
        {
            field = value;
            failedText.text = field.ToString();
        }
    }

    public int Enqueued
    {
        set
        {
            field = value;
            enqueuedText.text = field.ToString();
        }
    }
}
