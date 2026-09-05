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

        public static bool FuzzyStartsWith(string source, string prefix, bool caseSensitive = true)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(prefix))
                return false;

            source = RemoveAccent(source);
            prefix = RemoveAccent(prefix);
            
            if (!caseSensitive)
            {
                source = source.ToUpper();
                prefix = prefix.ToUpper();
            }
            
            List<string> sourceTokens = FuzzyTokenize(source);
            List<string> prefixTokens = FuzzyTokenize(prefix);

            if(sourceTokens.Count < prefixTokens.Count)
            {
                return false; // source has fewer tokens than prefix
            }

            int idx = 0;
            
            while (idx < prefixTokens.Count)
            {
                if (sourceTokens[idx] != prefixTokens[idx])
                    return false;

                idx++;
            }

            return true;
        }

        // Removes the prefix from the source string if it matches, otherwise returns an empty string.
        public static string FuzzyRemovePrefixOrEmpty(string source, string prefix, bool caseSensitive = true)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(prefix))
                return string.Empty;
            
            source = RemoveAccent(source);
            prefix = RemoveAccent(prefix);
            
            if (!caseSensitive)
            {
                source = source.ToUpper();
                prefix = prefix.ToUpper();
            }
            
            List<string> sourceTokens = FuzzyTokenize(source);
            List<string> prefixTokens = FuzzyTokenize(prefix);

            if(sourceTokens.Count < prefixTokens.Count)
            {
                return string.Empty; // source has fewer tokens than prefix
            }

            int idx = 0;

            while (idx < prefixTokens.Count)
            {
                if (sourceTokens[idx] != prefixTokens[idx])
                    return string.Empty; // mismatch found

                idx++;
            }

            return string.Join(" ", sourceTokens.Skip(idx));
        }

        public static bool FuzzyEndsWith(string source, string suffix, bool caseSensitive = true)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(suffix))
                return false;
            
            source = RemoveAccent(source);
            suffix = RemoveAccent(suffix);
            
            if (!caseSensitive)
            {
                source = source.ToUpper();
                suffix = suffix.ToUpper();
            }

            List<string> sourceTokens = FuzzyTokenize(source);
            List<string> suffixTokens = FuzzyTokenize(suffix);

            if(sourceTokens.Count < suffixTokens.Count)
            {
                return false; // source has fewer tokens than suffix
            }

            int sourceIdx = sourceTokens.Count - 1;
            int suffixIdx = suffixTokens.Count - 1;
            
            while (suffixIdx >= 0)
            {
                if (sourceTokens[sourceIdx] != suffixTokens[suffixIdx])
                    return false;
                
                sourceIdx--;
                suffixIdx--;
            }

            return true;
        }

        // Removes the suffix from the source string if it matches, otherwise returns an empty string.
        public static string FuzzyRemoveSuffixOrEmpty(string source, string suffix, bool caseSensitive = true)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(suffix))
                return string.Empty;
            
            source = RemoveAccent(source);
            suffix = RemoveAccent(suffix);
            
            if (!caseSensitive)
            {
                source = source.ToUpper();
                suffix = suffix.ToUpper();
            }
            
            List<string> sourceTokens = FuzzyTokenize(source);
            List<string> suffixTokens = FuzzyTokenize(suffix);

            if(sourceTokens.Count < suffixTokens.Count)
            {
                return string.Empty; // source has fewer tokens than suffix
            }

            int sourceIdx = sourceTokens.Count - 1;
            int suffixIdx = suffixTokens.Count - 1;
                        
            while (suffixIdx >= 0)
            {
                if (sourceTokens[sourceIdx] != suffixTokens[suffixIdx])
                    return string.Empty; // mismatch found

                sourceIdx--;
                suffixIdx--;
            }

            return string.Join(" ", sourceTokens.Take(sourceIdx + 1));
        }
    }
}
