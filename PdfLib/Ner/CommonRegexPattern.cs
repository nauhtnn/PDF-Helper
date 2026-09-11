using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfLib
{
    public static class CommonRegexPattern
    {
        public const string UppercaseWordsPattern =
            @"[A-ZÀ-Ỵ]+(\s+[A-ZÀ-Ỵ]+)*";
        public const string CorePersonNamePrefixPattern =
            @"([OÔ-Ộô-ộ]ng(\s*([/,]\s*[Bb][à-ạ]|\([Bb][à-ạ]\)))?|[Bb][à-ạ])";
        public const string LaxPersonNamePrefixPattern =
            @"([OÔ-Ộô-ộ]ng(\s*[/\(,]\s*[Bb][à-ạ]\)?)?|[Bb][à-ạ])\s*:?";
        public const string StrictPersonNamePrefixPattern =
            @"\b([Tt]ên\s+)?[Tt]ôi(\s+[Tt]ên)?(\s+[Ll][à-ạ])?\s*:";
        public const string PascalCasePattern =
            @"(\p{Lu}\p{Ll}+(\s+\p{Lu}\p{Ll}+)*)";
    }
}
