using UnityEngine.InputSystem;

namespace Jukebox.Input
{
    [ConfigureSingleton(SingletonFlags.PersistAutoInstance)]
    public class JukeboxInputs : MonoSingleton<JukeboxInputs>
    {
        private InputActionMap jukeboxActionMap;

        public void Awake()
        {
            jukeboxActionMap = InputManager.Instance.InputSource.Actions.asset.FindActionMap("Jukebox");

            Menu = jukeboxActionMap.FindAction("Jukebox Menu");
            Playback = jukeboxActionMap.FindAction("Playback Menu");
            NextTrack = jukeboxActionMap.FindAction("Next Track");
            RandomTrack = jukeboxActionMap.FindAction("Random Track");
            DisablePlayer = jukeboxActionMap.FindAction("Disable Player");
        }

        public InputAction Menu { get; private set; }
        public InputAction Playback { get; private set; }
        public InputAction NextTrack { get; private set; }
        public InputAction RandomTrack { get; private set; }
        public InputAction DisablePlayer { get; private set; }
    }
}