using System.Collections.Generic;
using Jukebox.RTLTMPro.Types;

namespace Jukebox.RTLTMPro;

internal static class LigatureFixer
{
    private static readonly List<int> LtrTextHolder = new List<int>(512);
    private static readonly List<int> TagTextHolder = new List<int>(512);
    private static readonly Dictionary<char, char> MirroredCharsMap = new Dictionary<char, char>()
    {
        ['('] = ')',
        [')'] = '(',
        ['»'] = '«',
        ['«'] = '»',
        ['{'] = '}',
        ['}'] = '{',
        ['['] = ']',
        [']'] = '[',
    };
    private static readonly HashSet<char> MirroredCharsSet = new HashSet<char>(MirroredCharsMap.Keys);
    private static void FlushBufferToOutput(List<int> buffer, FastStringBuilder output)
    {
        for (var j = 0; j < buffer.Count; j++)
        {
            output.Append(buffer[buffer.Count - 1 - j]);
        }

        buffer.Clear();
    }

    /// <summary>
    ///     Fixes the flow of the text.
    /// </summary>
    public static void Fix(FastStringBuilder input, FastStringBuilder output, bool farsi, bool fixTextTags, bool preserveNumbers)
    {
        // Some texts like tags, English words and numbers need to be displayed in their original order.
        // This list keeps the characters that their order should be reserved and streams reserved texts into final letters.
        LtrTextHolder.Clear();
        TagTextHolder.Clear();
        for (var i = input.Length - 1; i >= 0; i--)
        {
            var isInMiddle = i > 0 && i < input.Length - 1;
            var isAtBeginning = i == 0;
            var isAtEnd = i == input.Length - 1;

            var characterAtThisIndex = input.Get(i);

            int nextCharacter = default;
            if (!isAtEnd)
                nextCharacter = input.Get(i + 1);

            int previousCharacter = default;
            if (!isAtBeginning)
                previousCharacter = input.Get(i - 1);

            if (fixTextTags)
            {
                if (characterAtThisIndex == '>')
                {
                    // We need to check if it is actually the beginning of a tag.
                    var isValidTag = false;
                    var nextI = i;
                    TagTextHolder.Add(characterAtThisIndex);

                    for (var j = i - 1; j >= 0; j--)
                    {
                        var jChar = input.Get(j);
                            
                        TagTextHolder.Add(jChar);

                        if (jChar == '<')
                        {
                            var jPlus1Char = input.Get(j + 1);
                            // Tags do not start with space
                            if (jPlus1Char == ' ')
                            {
                                break;
                            }
                            isValidTag = true;
                            nextI = j;
                            break;
                        }
                    }

                    if (isValidTag)
                    {
                        FlushBufferToOutput(LtrTextHolder, output);
                        FlushBufferToOutput(TagTextHolder, output);
                        i = nextI;
                        continue;
                    } else
                    {
                        TagTextHolder.Clear();
                    }
                }
            }

            if (Char32Utils.IsPunctuation(characterAtThisIndex) || Char32Utils.IsSymbol(characterAtThisIndex))
            {
                if (MirroredCharsSet.Contains((char)characterAtThisIndex))
                {
                    // IsRTLCharacter returns false for null
                    var isAfterRTLCharacter = Char32Utils.IsRTLCharacter(previousCharacter);
                    var isBeforeRTLCharacter = Char32Utils.IsRTLCharacter(nextCharacter);

                    if (isAfterRTLCharacter || isBeforeRTLCharacter)
                    {
                        characterAtThisIndex = MirroredCharsMap[(char)characterAtThisIndex];
                    }
                }

                if (isInMiddle)
                {
                    var isAfterRTLCharacter = Char32Utils.IsRTLCharacter(previousCharacter);
                    var isBeforeRTLCharacter = Char32Utils.IsRTLCharacter(nextCharacter);
                    var isBeforeWhiteSpace = Char32Utils.IsWhiteSpace(nextCharacter);
                    var isAfterWhiteSpace = Char32Utils.IsWhiteSpace(previousCharacter);
                    var isUnderline = characterAtThisIndex == '_';
                    var isSpecialPunctuation = characterAtThisIndex == '.' ||
                                               characterAtThisIndex == '،' ||
                                               characterAtThisIndex == '؛';

                    if (isBeforeRTLCharacter && isAfterRTLCharacter ||
                        isAfterWhiteSpace && isSpecialPunctuation ||
                        isBeforeWhiteSpace && isAfterRTLCharacter ||
                        isBeforeRTLCharacter && isAfterWhiteSpace ||
                        (isBeforeRTLCharacter || isAfterRTLCharacter) && isUnderline)
                    {
                        FlushBufferToOutput(LtrTextHolder, output);
                        output.Append(characterAtThisIndex);
                    } else
                    {
                        LtrTextHolder.Add(characterAtThisIndex);
                    }
                } else if (isAtEnd)
                {
                    LtrTextHolder.Add(characterAtThisIndex);
                } else if (isAtBeginning)
                {
                    output.Append(characterAtThisIndex);
                }

                continue;
            }

            if (isInMiddle)
            {
                var isAfterEnglishChar = Char32Utils.IsEnglishLetter(previousCharacter);
                var isBeforeEnglishChar = Char32Utils.IsEnglishLetter(nextCharacter);
                var isAfterNumber = Char32Utils.IsNumber(previousCharacter, preserveNumbers, farsi);
                var isBeforeNumber = Char32Utils.IsNumber(nextCharacter, preserveNumbers, farsi);
                var isAfterSymbol = Char32Utils.IsSymbol(previousCharacter);
                var isBeforeSymbol = Char32Utils.IsSymbol(nextCharacter);

                // For cases where english words and farsi/arabic are mixed. This allows for using farsi/arabic, english and numbers in one sentence.
                // If the space is between numbers,symbols or English words, keep the order
                if (characterAtThisIndex == ' ' &&
                    (isBeforeEnglishChar || isBeforeNumber || isBeforeSymbol) &&
                    (isAfterEnglishChar || isAfterNumber || isAfterSymbol))
                {
                    LtrTextHolder.Add(characterAtThisIndex);
                    continue;
                }
            }

            if (Char32Utils.IsEnglishLetter(characterAtThisIndex) ||
                Char32Utils.IsNumber(characterAtThisIndex, preserveNumbers, farsi))
            {
                LtrTextHolder.Add(characterAtThisIndex);
                continue;
            }

            // handle surrogates
            if (characterAtThisIndex >= (char)0xD800 &&
                characterAtThisIndex <= (char)0xDBFF ||
                characterAtThisIndex >= (char)0xDC00 && characterAtThisIndex <= (char)0xDFFF)
            {
                LtrTextHolder.Add(characterAtThisIndex);
                continue;
            }

            FlushBufferToOutput(LtrTextHolder, output);

            if (characterAtThisIndex != 0xFFFF &&
                characterAtThisIndex != (int)SpecialCharacters.ZeroWidthNoJoiner)
            {
                output.Append(characterAtThisIndex);
            }
        }

        FlushBufferToOutput(LtrTextHolder, output);
    }
}