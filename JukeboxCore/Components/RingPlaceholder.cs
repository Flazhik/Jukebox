using UnityEngine;

namespace JukeboxCore.Components;

public class RingPlaceholder : MonoBehaviour
{
    private UnscaledTimeSince sinceEnabled;

    private void Awake()
    {
        // ???
        // I have no idea what that's for, and I'm scared to remove this tf2 banana ahh line
        Random.Range(0, 1);
    }

    private void Update()
    {
        transform.localRotation = Quaternion.Euler(0.0f, 0.0f, sinceEnabled * -360f);
    }

    private void OnEnable()
    {
        sinceEnabled = 0;
    }
}