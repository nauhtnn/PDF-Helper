using DocumentFormat.OpenXml.Bibliography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PdfLib
{
    public static class TextMeasurement
    {
        public static string RemoveAccent(string input)
        {
            StringBuilder NoAccent = new StringBuilder();
            foreach(char c in input.Normalize(NormalizationForm.FormD))
            {
                if(System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    NoAccent.Append(c);
                }
            }

            return NoAccent.ToString();
        }

        public static string SentenceCase(string sentence)
        {
            if (string.IsNullOrEmpty(sentence))
                return sentence;

            if(sentence.Length == 1)
                return sentence.ToUpper();

            sentence = sentence.ToLower();
            return char.ToUpper(sentence[0]) + sentence.Substring(1);
        }

        public static List<string> FuzzyTokenize(string input)
        {
            if (string.IsNullOrEmpty(input))
                return new List<string>();

            var matches = Regex.Matches(input, @"\w+|[^\w\s]");

            // Convert to List<string>
            List<string> tokens = matches.Cast<Match>()
                                         .Select(m => m.Value)
                                         .ToList();

            return tokens;
        }

        public static bool FuzzyStartWith(string source, string prefix, bool caseSensitive = true)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(prefix))
                return false;
            source = RemoveAccent(source);
            prefix = RemoveAccent(prefix);
            if (!caseSensitive)
            {
                source = source.ToLower();
                prefix = prefix.ToLower();
            }
            List<string> sourceTokens = FuzzyTokenize(source);
            List<string> prefixTokens = FuzzyTokenize(prefix);

            int sourceIdx = 0;
            int prefixIdx = 0;
            while (sourceIdx < sourceTokens.Count && prefixIdx < prefixTokens.Count)
            {
                if (sourceTokens[sourceIdx] != prefixTokens[prefixIdx])
                {
                    return false;
                }
                sourceIdx++;
                prefixIdx++;
            }

            if(prefixIdx < prefixTokens.Count)
            {
                return false; // prefix has more tokens than source
            }

            return true;
        }

        public static string FuzzryRemovePrefix(string source, string prefix, bool caseSensitive = true)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(prefix))
                return source;
            source = RemoveAccent(source);
            prefix = RemoveAccent(prefix);
            if (!caseSensitive)
            {
                source = source.ToLower();
                prefix = prefix.ToLower();
            }
            List<string> sourceTokens = FuzzyTokenize(source);
            List<string> prefixTokens = FuzzyTokenize(prefix);
            int sourceIdx = 0;
            int prefixIdx = 0;
            while (sourceIdx < sourceTokens.Count && prefixIdx < prefixTokens.Count)
            {
                if (sourceTokens[sourceIdx] != prefixTokens[prefixIdx])
                {
                    break;
                }
                sourceIdx++;
                prefixIdx++;
            }
            if (prefixIdx < prefixTokens.Count)
            {
                return string.Empty; // prefix has more tokens than source
            }

            return string.Join(" ", sourceTokens.Skip(sourceIdx));
        }

        public static bool FuzzyEndsWith(string source, string suffix, bool caseSensitive = true)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(suffix))
                return false;
            source = RemoveAccent(source);
            suffix = RemoveAccent(suffix);
            if (!caseSensitive)
            {
                source = source.ToLower();
                suffix = suffix.ToLower();
            }

            List<string> sourceTokens = FuzzyTokenize(source);
            List<string> suffixTokens = FuzzyTokenize(suffix);

            int sourceIdx = sourceTokens.Count - 1;
            int suffixIdx = suffixTokens.Count - 1;
            while (sourceIdx >= 0 && suffixIdx >= 0)
            {
                if (sourceTokens[sourceIdx] != suffixTokens[suffixIdx])
                {
                    return false;
                }
                sourceIdx--;
                suffixIdx--;
            }

            if (suffixIdx >= 0)
            {
                return false; // suffix has more tokens than source
            }

            return true;
        }

        public static string FuzzyRemoveSuffix(string source, string suffix, bool caseSensitive = true)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(suffix))
                return source;
            source = RemoveAccent(source);
            suffix = RemoveAccent(suffix);
            if (!caseSensitive)
            {
                source = source.ToLower();
                suffix = suffix.ToLower();
            }
            List<string> sourceTokens = FuzzyTokenize(source);
            List<string> suffixTokens = FuzzyTokenize(suffix);
            int sourceIdx = sourceTokens.Count - 1;
            int suffixIdx = suffixTokens.Count - 1;
            while (sourceIdx >= 0 && suffixIdx >= 0)
            {
                if (sourceTokens[sourceIdx] != suffixTokens[suffixIdx])
                {
                    break;
                }
                sourceIdx--;
                suffixIdx--;
            }
            if (suffixIdx >= 0)
            {
                return string.Empty; // suffix has more tokens than source
            }
            return string.Join(" ", sourceTokens.Take(sourceIdx + 1));
        }
    }
}
