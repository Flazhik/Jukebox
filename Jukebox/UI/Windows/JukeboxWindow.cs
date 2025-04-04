using Jukebox.Components;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Jukebox.UI.Windows
{
    public abstract class JukeboxWindow : MonoBehaviour
    {
        [SerializeField]
        public InputActionReference hotkey;

        public abstract string StateKey();

        public void Open()
        {
            gameObject.SetActive(true);
            JukeboxPauseManager.Instance.Pause(StateKey(), gameObject);
        }

        public void Close()
        {
            GameStateManager.Instance.PopState(StateKey());
            gameObject.SetActive(false);
        }

        public void Toggle()
        {
            if (IsOpened())
                Close();
            else
                Open();
        }

        protected virtual void Awake()
        {
            if (hotkey != null)
                hotkey = InputActionReference.Create(InputManager.Instance.InputSource.Actions.asset.FindAction(hotkey.ToInputAction().id));
        }

        public bool IsOpened() => gameObject.activeSelf;
    }
}