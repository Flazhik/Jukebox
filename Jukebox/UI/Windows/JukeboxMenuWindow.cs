using UnityEngine;
using UnityEngine.UI;

namespace Jukebox.UI.Windows
{
    public class JukeboxMenuWindow : JukeboxWindow
    {
        [SerializeField]
        protected Button[] buttons;

        [SerializeField]
        protected GameObject activeBgObject;

        public override string StateKey() => "jukeboxMenu";

        protected override void Awake()
        {
            base.Awake();
            foreach (var button in buttons)
            {
                button.onClick.AddListener(() =>
                {
                    activeBgObject.transform.SetParent(button.transform);
                    activeBgObject.transform.SetSiblingIndex(0);
                    activeBgObject.transform.localPosition = Vector3.zero;
                });
            }
        }
    }
}