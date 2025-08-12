using TMPro;
using UnityEngine;

namespace JukeboxDownloader.UI
{
    public class TotalDownloadProgress : MonoBehaviour
    {
        [SerializeField]
        public TMP_Text downloadedText;
        
        [SerializeField]
        public TMP_Text failedText;
        
        [SerializeField]
        public TMP_Text enqueuedText;

        public int Downloaded
        {
            set
            {
                mDownloaded = value;
                downloadedText.text = mDownloaded.ToString();
            }
        }
        
        public int Failed
        {
            set
            {
                mFailed = value;
                failedText.text = mFailed.ToString();
            }
        }
        
        public int Enqueued
        {
            set
            {
                mEnqueued = value;
                enqueuedText.text = mEnqueued.ToString();
            }
        }

        private int mDownloaded;
        private int mFailed;
        private int mEnqueued;
    }
}