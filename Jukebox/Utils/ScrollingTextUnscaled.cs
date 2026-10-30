using System.Collections;
using TMPro;
using UnityEngine;

namespace Jukebox.Utils;

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
                while (currentLetter < message.Length && message[currentLetter] != '>')
                    ++currentLetter;
            else if (currentLetter < message.Length - 1)
                while (currentLetter < message.Length - 1 && message[currentLetter + 1] == ' ')
                    ++currentLetter;

            ++currentLetter;
            text.text = message[..currentLetter];
            if (clickAudio != null && message[currentLetter - 1] != '\n' && message[currentLetter - 1] != ' ')
                clickAudio.Play();
            yield return new WaitForSecondsRealtime(secondsBetweenLetters);
        }
    }

    public static IEnumerator HideText(
        TMP_Text text,
        float secondsBetweenLetters = 0.005f)
    {
        var message = text.text;
        var currentLetter = message.Length - 1;

        while (currentLetter >= 0)
        {
            if (message[currentLetter] == '>')
                while (currentLetter >= 0 && message[currentLetter] != '<')
                    --currentLetter;

            --currentLetter;
            text.text = message[..(currentLetter + 1)];
            yield return new WaitForSecondsRealtime(secondsBetweenLetters);
        }
    }

    public static IEnumerator Reveal(TMP_Text text, float secondsBetweenLetters = 0.005f, AudioSource clickAudio = null)
    {
        text.ForceMeshUpdate();

        var total = text.textInfo.characterCount;
        var visible = 0;
        text.maxVisibleCharacters = 0;

        while (visible < total)
        {
            visible++;
            text.maxVisibleCharacters = visible;

            if (clickAudio != null)
                clickAudio.Play();

            yield return new WaitForSecondsRealtime(secondsBetweenLetters);
        }
    }

    public static IEnumerator Conceal(TMP_Text text, float secondsBetweenLetters = 0.005f)
    {
        var visible = text.maxVisibleCharacters;

        while (visible > 0)
        {
            visible--;
            text.maxVisibleCharacters = visible;
            yield return new WaitForSecondsRealtime(secondsBetweenLetters);
        }
    }
}
