using System.Linq;
using UnityEngine.InputSystem;

namespace Jukebox.Input
{
    [ConfigureSingleton(SingletonFlags.PersistAutoInstance)]
    public class JukeboxInputs : MonoSingleton<JukeboxInputs>
    {
        private InputActionMap jukeboxActionMap;

        protected new void Awake()
        {
            base.Awake();
            jukeboxActionMap = InputManager.Instance.InputSource.Actions.asset.FindActionMap("Jukebox");

            Menu = jukeboxActionMap.FindAction("Jukebox Menu");
            Playback = jukeboxActionMap.FindAction("Playback Menu");
            NextTrack = jukeboxActionMap.FindAction("Next Track");
            DisablePlayer = jukeboxActionMap.FindAction("Disable Player");
            ResetM1Bindings();
        }

        // Legacy costs for my own stupidity
        private void ResetM1Bindings()
        {
            ResetLeftClick(Menu, "<Keyboard>/f4");
            ResetLeftClick(Playback, "<Keyboard>/backquote");
            ResetLeftClick(NextTrack, "<Keyboard>/f3");
            ResetLeftClick(DisablePlayer, "<Keyboard>/f10");
        }

        private void ResetLeftClick(InputAction binding, string defaultBinding)
        {
            if (binding.bindings.Any(b => b.path == "<Mouse>/leftButton"))
            {
                binding.ChangeBinding(Menu.GetBindingIndex())
                    .WithPath(defaultBinding)
                    .WithGroup("Keyboard & Mouse");
            }
        }

        public InputAction Menu { get; private set; }
        public InputAction Playback { get; private set; }
        public InputAction NextTrack { get; private set; }
        public InputAction DisablePlayer { get; private set; }
    }
}