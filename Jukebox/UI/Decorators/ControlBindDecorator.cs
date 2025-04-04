using UnityEngine;
using UnityEngine.InputSystem;

namespace Jukebox.UI.Decorators
{
    public class ControlBindDecorator: MonoBehaviour
    {
        [SerializeField]
        public InputActionReference action;

        protected void Awake()
        {
            var ukAction = InputManager.Instance.InputSource.Actions.asset.FindAction(action.ToInputAction().id);
            var bind = GetComponentInChildren<ControlsOptionsKey>();
            bind.RebuildBindings(ukAction, InputManager.Instance.InputSource.Actions.KeyboardMouseScheme);
        }
    }
}
