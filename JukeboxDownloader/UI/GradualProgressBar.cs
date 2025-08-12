using System;
using UnityEngine;
using UnityEngine.UI;

namespace JukeboxDownloader.UI
{
    public class GradualProgressBar : MonoBehaviour
    {
        private const float Epsilon = 0.001f;
        
        [SerializeField]
        public Slider progressBar;
        
        [SerializeField]
        public float smoothTime;
        
        private float progressBarTargetValue;
        private float progressBarVelocity;
        
        private void Update()
        {
            var difference = progressBarTargetValue - progressBar.value;
            
            if (Math.Abs(difference) < Epsilon)
                return;

            if (difference > 0)
                progressBar.value = Mathf.SmoothDamp(
                    progressBar.value,
                    progressBarTargetValue,
                    ref progressBarVelocity,
                    smoothTime,
                    float.PositiveInfinity,
                    Time.unscaledDeltaTime);
            else
                progressBar.value = progressBarTargetValue;
        }
        
        public void UpdateProgress(float progress)
        {
            progressBarTargetValue = progress;
        }
        
        public void ForceSetProgress(float progress)
        {
            progressBarTargetValue = progress;
            progressBar.value = progressBarTargetValue;
        }
    }
}