using UnityEngine;

namespace Jukebox.UI.Windows
{
    public class WhatsNew : JukeboxWindow
    {
        private const string PrefsKey =  "Jukebox.NewDiscordServer";
        
        public override string StateKey() => "jukebox.whatisnew";

        private void Start()
        {
            if (PlayerPrefs.GetInt(PrefsKey) != 1)
                PlayerPrefs.SetInt(PrefsKey, 1);
            else
                Close();
        }
    }
}