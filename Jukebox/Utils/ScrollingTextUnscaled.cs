using System.Collections;
using TMPro;
using UnityEngine;

namespace Jukebox.Utils
{
    public static class ScrollingTextUnscaled
    {
        public static IEnumerator ShowText(
            TMP_Text text,
            string message,
            float secondsBetweenLetters = 0.005f,
            AudioSource clickAudio = null)
        {
            var currentLetter = 0;
            text.text = "";
            while (currentLetter < message.Length)
            {
                if (message[currentLetter] == '<')
                {
                    while (message[currentLetter] != '>' && currentLetter <= message.Length)
                        ++currentLetter;
                }
                else if (currentLetter < message.Length - 1)
                {
                    while (currentLetter < message.Length - 1 && message[currentLetter + 1] == ' ')
                        ++currentLetter;
                }

                ++currentLetter;
                text.text = message.Substring(0, currentLetter);
                if (clickAudio != null && message[currentLetter - 1] != '\n' && message[currentLetter - 1] != ' ')
                    clickAudio.Play();
                yield return new WaitForSecondsRealtime(secondsBetweenLetters);
            }
        }
        
        public static IEnumerator HideText(
            TMP_Text text,
            float secondsBetweenLetters = 0.005f)
        {
            var currentLetter = text.text.Length - 1;
            var message = text.text;

            while (currentLetter >= 0)
            {
                if (text.text[currentLetter] == '>')
                    while (text.text[currentLetter] != '<' && currentLetter >= 0)
                        --currentLetter;

                --currentLetter;
                text.text = message.Substring(0, currentLetter + 1);
                yield return new WaitForSecondsRealtime(secondsBetweenLetters);
            }
        }
    }
}