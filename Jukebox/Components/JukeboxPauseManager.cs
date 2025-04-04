using UnityEngine;

namespace Jukebox.Components
{
    [ConfigureSingleton(SingletonFlags.HideAutoInstance)]
    public class JukeboxPauseManager : MonoSingleton<JukeboxPauseManager>
    {
        public void Pause(string stateKey, GameObject window)
        {
            NewMovement.Instance.enabled = false;
            CameraController.Instance.activated = false;
            GunControl.Instance.activated = false;
            GameStateManager.Instance.RegisterState(new GameState(stateKey, new[] { window })
            {
                cursorLock = LockMode.Unlock,
                cameraInputLock = LockMode.Lock,
                playerInputLock = LockMode.Lock
            });

            OptionsManager.Instance.paused = true;
        }

        public void UnPause()
        {
            Time.timeScale = MonoSingleton<TimeController>.Instance.timeScale *
                             MonoSingleton<TimeController>.Instance.timeScaleModifier;
            OptionsManager.Instance.paused = false;
            CameraController.Instance.activated = true;
            NewMovement.Instance.enabled = true;
            GunControl.Instance.activated = true;
        }
    }
}