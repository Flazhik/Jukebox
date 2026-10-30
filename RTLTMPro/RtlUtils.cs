using System.Text;
using TMPro;

namespace Jukebox.RTLTMPro;

public static class RtlUtils
{
    private enum RtlLanguage
    {
        None,
        Arabic,
        Farsi,
        Hebrew,
    }
    
    public static string Set(TMP_Text component, string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var language = DetectLanguage(text);
        var fixedText = Fix(text, language);
        component.isRightToLeftText = language != RtlLanguage.None;
        component.text = fixedText;
        return fixedText;
    }
        
    private static string Fix(string text, RtlLanguage language)
    {
        if (language == RtlLanguage.None)
            return text;
        
        var farsi = language == RtlLanguage.Farsi;

        if (language == RtlLanguage.Hebrew)
            text = MoveHebrewMarksBeforeBase(text);

        var sb = new FastStringBuilder(RTLSupport.DefaultBufferSize);
        RTLSupport.FixRTL(text, sb, farsi, fixTextTags: true, preserveNumbers: true);
        sb.Reverse();
        return sb.ToString();
    }

    private static string MoveHebrewMarksBeforeBase(string text)
    {
        var result = new StringBuilder(text.Length);
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];

            if (!IsHebrewLetter(c))
            {
                result.Append(c);
                continue;
            }

            var j = i + 1;
            while (j < text.Length && IsHebrewMark(text[j]))
                j++;

            for (var k = i + 1; k < j; k++)
                result.Append(text[k]);

            result.Append(c);
            i = j - 1;
        }

        return result.ToString();
    }
    
    private static RtlLanguage DetectLanguage(string text)
    {
        var hasHebrew = false;
        var hasFarsi = false;
        var hasArabic = false;

        foreach (var c in text)
        {
            if (c is >= '\u0590' and <= '\u05FF')
                hasHebrew = true;
            else if (IsFarsiSpecific(c))
                hasFarsi = true;
            else if (IsArabicCharacter(c))
                hasArabic = true;
        }

        if (hasHebrew)
            return RtlLanguage.Hebrew;
        if (hasFarsi)
            return RtlLanguage.Farsi;
        if (hasArabic)
            return RtlLanguage.Arabic;
        return RtlLanguage.None;
    }

    private static bool IsHebrewLetter(char c) =>
        c is >= '\u05D0' and <= '\u05EA' or >= '\u05F0' and <= '\u05F2';
    
    private static bool IsHebrewMark(char c) => 
        c is >= '\u0591' and <= '\u05BD' or '\u05BF' or '\u05C1' or '\u05C2' or '\u05C4' or '\u05C5' or '\u05C7';
    
    private static bool IsFarsiSpecific(char c) => c switch
    {
        '\u06CC' or '\u067E' or '\u0686' or '\u0698' or '\u06A9' or '\u06AF' => true,
        _ => false
    };

    private static bool IsArabicCharacter(char c) =>
        c is >= '\u0600' and <= '\u06FF' or >= '\u0750' and <= '\u077F' or >= '\u0870' and <= '\u089F'
            or >= '\u08A0' and <= '\u08FF' or >= '\uFB50' and <= '\uFDFF' or >= '\uFE70' and <= '\uFEFF';
    
}