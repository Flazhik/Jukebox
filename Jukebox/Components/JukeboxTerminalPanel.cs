using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.GameObject;
using static Jukebox.Utils.ReflectionUtils;

namespace Jukebox.Components
{
    public class JukeboxTerminalPanel: MonoBehaviour
    {
        private const string CybergrindShopPath = "/FirstRoom/Room/Cybergrind Shop";

        [SerializeField]
        public GameObject musicButton;        
        
        [SerializeField]
        public GameObject sfxButton;
        
        [SerializeField]
        public GameObject[] newPanels;
        
        private CyberGrindSettingsNavigator settingsNavigator;

        protected void Awake()
        {
            var cybergrindShop = Find(CybergrindShopPath);
            settingsNavigator = cybergrindShop.GetComponent<CyberGrindSettingsNavigator>();
            var mainPanel = Find($"{CybergrindShopPath}/Canvas/Background/Main Panel");
            var shopZone = cybergrindShop.GetComponent<ShopZone>();
            DisableShopExitTrigger();

            var buttons = mainPanel.transform.Find("Main Menu/Buttons");
            Destroy(buttons.Find("Music Button").gameObject);
            musicButton.transform.SetParent(buttons, false);
            sfxButton.transform.SetParent(buttons, false);

            DestroyPanel("Songs Custom");
            DestroyPanel("Songs Type Selection");
            DestroyPanel("Songs Soundtrack");
            DestroyPanel("Playlist");

            settingsNavigator.allPanels = settingsNavigator.allPanels.Concat(newPanels).ToArray();

            void DestroyPanel(string path)
            {
                var panel = mainPanel.transform.Find(path).gameObject;
                settingsNavigator.allPanels = settingsNavigator.allPanels.Where(obj => obj != panel).ToArray();
                Destroy(mainPanel.transform.Find(path).gameObject);
            }

            void DisableShopExitTrigger() =>
                SetPrivate<ScreenZone, UnityEvent>(shopZone, typeof(ScreenZone), "onExitZone", new UnityEvent());
        }

        public void GoToStats() => settingsNavigator.GoTo(settingsNavigator.tipPanel);

        public void GoTo(GameObject panel) => settingsNavigator.GoTo(panel);
        
        public void GoToNoMenu(GameObject panel) => settingsNavigator.GoToNoMenu(panel);
    }
}