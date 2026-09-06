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
                    if(c == 'Đ') NoAccent.Append('D'); // Handle uppercase Đ
                    else if (c == 'đ') NoAccent.Append('d'); // Handle lowercase đ
                    else NoAccent.Append(c);
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

        public static List<string> Tokenize(string input)
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
            
            List<string> sourceTokens = Tokenize(source);
            List<string> prefixTokens = Tokenize(prefix);

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
        public static bool FuzzyRemovePrefix(string source, string prefix,
            out string removed, out string remaining, bool caseSensitive = true)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(prefix))
            {
                removed = string.Empty;
                remaining = source;
                return false;
            }
            
            string fuzzySource = RemoveAccent(source);
            string fuzzyPrefix = RemoveAccent(prefix);
            
            if (!caseSensitive)
            {
                fuzzySource = fuzzySource.ToUpper();
                fuzzyPrefix = fuzzyPrefix.ToUpper();
            }
            
            List<string> fuzzySourceTokens = Tokenize(fuzzySource);
            List<string> fuzzyPrefixTokens = Tokenize(fuzzyPrefix);

            if(fuzzySourceTokens.Count < fuzzyPrefixTokens.Count)
            {
                removed = string.Empty;
                remaining = source;
                return false; // source has fewer tokens than prefix
            }

            int idx = 0;

            while (idx < fuzzyPrefixTokens.Count)
            {
                if (fuzzySourceTokens[idx] != fuzzyPrefixTokens[idx])
                {
                    removed = string.Empty;
                    remaining = source;
                    return false; // mismatch found
                }

                idx++;
            }

            List<string> sourceTokens = Tokenize(source);
            removed = string.Join(" ", sourceTokens.Take(idx));
            remaining = string.Join(" ", sourceTokens.Skip(idx));
            return true;
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

            List<string> sourceTokens = Tokenize(source);
            List<string> suffixTokens = Tokenize(suffix);

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
        public static bool FuzzyRemoveSuffix(string source, string suffix,
            out string removed, out string remaining, bool caseSensitive = true)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(suffix))
            {
                removed = string.Empty;
                remaining = source;
                return false;
            }

            string fuzzySource = RemoveAccent(source);
            string fuzzySuffix = RemoveAccent(suffix);
            
            if (!caseSensitive)
            {
                fuzzySource = fuzzySource.ToUpper();
                fuzzySuffix = fuzzySuffix.ToUpper();
            }
            
            List<string> fuzzySourceTokens = Tokenize(fuzzySource);
            List<string> fuzzySuffixTokens = Tokenize(fuzzySuffix);

            if(fuzzySourceTokens.Count < fuzzySuffixTokens.Count)
            {
                removed = string.Empty;
                remaining = source;
                return false; // source has fewer tokens than suffix
            }

            int sourceIdx = fuzzySourceTokens.Count - 1;
            int suffixIdx = fuzzySuffixTokens.Count - 1;
                        
            while (suffixIdx >= 0)
            {
                if (fuzzySourceTokens[sourceIdx] != fuzzySuffixTokens[suffixIdx])
                {
                    removed = string.Empty;
                    remaining = source;
                    return false; // mismatch found
                }

                sourceIdx--;
                suffixIdx--;
            }

            List<string> sourceTokens = Tokenize(source);
            removed = string.Join(" ", sourceTokens.Take(sourceIdx + 1));
            remaining = string.Join(" ", sourceTokens.Skip(sourceIdx + 1));
            return true;
        }
    }
}
