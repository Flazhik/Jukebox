using UnityEngine;

namespace JukeboxCore.Components
{
    public class RingPlaceholder : MonoBehaviour
    {
        private UnscaledTimeSince sinceEnabled;

        private void Awake() => Random.Range(0, 1);

        private void OnEnable() => sinceEnabled = 0;

        private void Update() => transform.localRotation = Quaternion.Euler(0.0f, 0.0f, sinceEnabled * -360f);
    }
}