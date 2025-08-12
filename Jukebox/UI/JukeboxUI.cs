using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Jukebox.Components;
using Jukebox.UI.Windows;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GameObject;

namespace Jukebox.UI
{
    public class JukeboxUI : MonoBehaviour
    {
        [SerializeField] public GameObject jukeboxMenuTemplate;
        [SerializeField] public GameObject playbackMenuTemplate;
        [SerializeField] public GameObject nowPlayingHudTemplate;
        [SerializeField] public GameObject nowPlayingHudClassicTemplate;

        private GameObject jukeboxMenu;
        private GameObject playbackMenu;

        private ReadOnlyCollection<JukeboxWindow> windows;

        public void OpenDownloader()
        {
            var jukeboxWindow = jukeboxMenu.GetComponent<JukeboxMenuWindow>();
            jukeboxWindow.Open();
            jukeboxWindow.OpenDownloader();
        }

        protected void Awake()
        {
            InstantiateUI();
            windows = new ReadOnlyCollection<JukeboxWindow>(new List<JukeboxWindow>
            {
                jukeboxMenu.GetComponent<JukeboxWindow>(),
                playbackMenu.GetComponent<JukeboxPlaybackWindow>()
            });

            foreach (var window in windows)
                window.gameObject.SetActive(false);
        }

        protected void Update()
        {
            ManageWindows();
        }

        private void ManageWindows()
        {
            if (GameIsPaused())
                return;
            
            if (InputManager.Instance.InputSource.Pause.WasPerformedThisFrame)
            {
                foreach (var window in windows)
                    if (window.IsOpened())
                        window.Close();
                return;
            }

            var requestedWindow = windows
                .FirstOrDefault(w => w.hotkey.ToInputAction().WasPerformedThisFrame());

            if (requestedWindow != default)
            {
                foreach (var window in windows)
                    if (window.hotkey.ToInputAction().IsActionEqual(requestedWindow.hotkey.ToInputAction()))
                        window.Toggle();
                    else
                        window.Close();
            }
            
            if (!windows.Any(w => GameStateManager.Instance.IsStateActive(w.StateKey()))
                && OptionsManager.Instance.paused)
                JukeboxPauseManager.Instance.UnPause();
        }

        private void InstantiateUI()
        {
            var newCanvas = new GameObject
            {
                name = "JukeboxCanvas"
            };
            newCanvas.AddComponent<Canvas>();

            var canvas = Find("/Canvas");
            var canvasComponent = newCanvas.GetComponent<Canvas>();
            canvasComponent.renderMode = RenderMode.ScreenSpaceCamera;
            canvasComponent.sortingOrder = canvas.GetComponent<Canvas>().sortingOrder + 1;
            newCanvas.AddComponent<CanvasScaler>();
            newCanvas.AddComponent<GraphicRaycaster>();

            jukeboxMenu = Instantiate(jukeboxMenuTemplate, canvas.transform);
            playbackMenu = Instantiate(playbackMenuTemplate, canvas.transform);
            Instantiate(nowPlayingHudTemplate, Find("/Player/Main Camera/HUD Camera/HUD/GunCanvas").transform, false);
            Instantiate(nowPlayingHudClassicTemplate, canvas.transform.Find("Crosshair Filler"), false);
        }

        private static bool GameIsPaused() => GameStateManager.Instance.IsStateActive("pause");
    }
}