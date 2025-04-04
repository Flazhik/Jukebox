using System.Collections;
using Jukebox.Core.Model.Song;
using UnityEngine;

namespace Jukebox.UI.SongPanel
{
    public class ClassicSongPanel: SongPanel
    {
        protected override SongPanelStyle Style => SongPanelStyle.Classic;
        
        protected override IEnumerator FadeIn(JukeboxSongMetadata metadata)
        {
            var time = 0.0f;
            while (time < (double)panelApproachTime)
            {
                time += Time.unscaledDeltaTime;
                panelGroup.alpha = active ? time / panelApproachTime : 0;
                yield return null;
            }
            panelGroup.alpha = active ? 1 : 0;
        }

        protected override IEnumerator Await()
        {
            var time = panelStayTime;
            while (time > 0.0 || showIndefinitely)
            {
                panelGroup.alpha = active ? 1 : 0;
                if (time > 0.0) 
                    time -= Time.unscaledDeltaTime;
                yield return null;
            }
        }

        protected override IEnumerator FadeOut()
        {
            var time = panelApproachTime;
            while (time > 0.0)
            {
                time -= Time.unscaledDeltaTime;
                panelGroup.alpha = active ? time / panelApproachTime : 0;
                yield return null;
            }
            panelGroup.alpha = 0.0f;
        }
    }
}