using UnityEngine;
using static UnityEngine.GameObject;
using static UnityEngine.Resources;

namespace Jukebox.Components
{
    [ConfigureSingleton(SingletonFlags.NoAutoInstance)]
    public class JukeboxManager : MonoSingleton<JukeboxManager>
    {
        private const string CanvasPath = "/Canvas";
        private const string CybergrindShopPath = "/FirstRoom/Room/Cybergrind Shop";
        private const string PanelPath = "/FirstRoom/Room/Cybergrind Shop/Canvas/Background/Main Panel";

        [SerializeField]
        public GameObject songPanel;

        [SerializeField]
        public JukeboxPlaylistEditor playlistEditor;
        
        [SerializeField]
        public JukeboxMusicPlayer player;

        [SerializeField]
        private GameObject terminalPanel;
        
        [SerializeField]
        private GameObject musicLogic;
        
        [SerializeField]
        private GameObject musicChanger;

        protected new void Awake()
        {
            base.Awake();
            ReplaceOriginalLogic();
            SetupTerminal();
            SetupSongPanel();
        }

        private void SetupTerminal()
        {
            var mainPanel = Find($"{PanelPath}");
            terminalPanel.transform.SetParent(mainPanel.transform, false);
        }

        private void ReplaceOriginalLogic()
        {
            var cybergrindShop = Find(CybergrindShopPath);
            
            var originalLogic = cybergrindShop.transform.Find("Logic");
            Destroy(originalLogic.Find("Music").gameObject);
            musicLogic.transform.parent = originalLogic;

            var originalChanger = FindObjectsOfTypeAll<CustomMusicPlayer>()[0];
            var timer = originalChanger.transform.parent;
            Destroy(originalChanger.gameObject);
            musicChanger.transform.parent = timer.transform;
        }

        private void SetupSongPanel()
        {
            var canvas = Find(CanvasPath);
            Destroy(canvas.transform.Find("SongPanel").gameObject);
            songPanel.transform.SetParent(canvas.transform, false);
        }
    }
}