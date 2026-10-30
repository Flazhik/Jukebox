using UnityEngine;

namespace Jukebox.UI.Windows;

public class WhatsNew : JukeboxWindow
{
    private const string PrefsKey = "Jukebox.2.3.0-Release";

    private void Start()
    {
        if (PlayerPrefs.GetInt(PrefsKey) != 1)
            PlayerPrefs.SetInt(PrefsKey, 1);
        else
            Close();
    }

    public override string StateKey() => "jukebox.whatisnew";
}