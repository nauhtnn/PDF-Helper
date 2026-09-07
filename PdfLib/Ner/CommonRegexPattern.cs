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
            @"([Ôô]ng(\s*([/,]\s*[Bb]à|\([Bb]à\)))?|[Bb]à)";
        public const string LaxPersonNamePrefixPattern =
            @"([Ôô]ng(\s*[/\(,]\s*[Bb]à\)?)?|[Bb]à)\s*:?";
        public const string StrictPersonNamePrefixPattern =
            @"\b([Tt]ên\s+)?[Tt]ôi(\s+[Tt]ên)?(\s+[Ll]à)?\s*:";
        public const string PascalCasePattern =
            @"([A-ZÀ-Ỵ][a-zà-ỵ]+(\s+[A-ZÀ-Ỵ][a-zà-ỵ]+)*)";
    }
}
