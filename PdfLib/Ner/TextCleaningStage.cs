using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PdfLib
{
    public class TextCleaningStage : PipelineStage
    {
        public TextCleaningStage()
            : base("Text Cleaning", "Cleans and normalizes text") { }

        bool HandlePrefixes(string[] prefixes, string line, StringBuilder paragraph, List<string> paragraphs, bool isSeparatedLineForPrefix)
        {
            foreach (var prefix in prefixes)
            {
                string removed = null;
                string remaining = null;
                bool found;
                
                if (isSeparatedLineForPrefix)
                    found = TextMeasurement.FuzzyRemovePrefix(line, prefix, out removed, out remaining);
                else
                    found = TextMeasurement.FuzzyStartsWith(line, prefix);
                
                if (found)
                {
                    if (paragraph.Length > 0)
                    {
                        paragraphs.Add(paragraph.ToString());
                        paragraph.Clear();
                    }

                    if (isSeparatedLineForPrefix)
                    {
                        paragraphs.Add(removed);
                        if(!string.IsNullOrEmpty(remaining))
                            paragraphs.Add(remaining);
                    }
                    else
                        paragraphs.Add(line);

                    return true;
                }
            }

            return false;
        }

        public override BaseEntity Execute(BaseEntity input)
        {
            LineFragment lineFragment = input as LineFragment;
            
            if (lineFragment == null)
            {
                throw new ArgumentException("Input must be of type LineFragment", nameof(input));
            }

            StringBuilder paragraph = new StringBuilder();

            List<Page> pages = new List<Page>();
            pages.Add(new Page());

            List<Document> documents = new List<Document>();

            bool isNewParagraph = true;

            bool hasDocumentTitle = false;

            int i = 0;

            List<DocumentType> docTypes = new List<DocumentType>();

            while (i < lineFragment.Lines.Length)
            {
                string line = lineFragment.Lines[i].Trim();
                i++;
                
                if (line.Length == 0)
                    continue;

                if(Regex.IsMatch(line, @"<TRANG \d+ />"))
                {
                    if(paragraph.Length > 0)
                    {
                        pages.Last().Paragraphs.Add(paragraph.ToString());
                        paragraph.Clear();
                    }

                    if(hasDocumentTitle)
                    {
                        Document doc = new Document();

                        if(docTypes.Count > 0)
                        {
                            doc.DocTypes = docTypes;
                            docTypes = new List<DocumentType>();
                        }
                        
                        foreach (var page in pages)
                            doc.TextBlock.AddRange(page.Paragraphs);
                        if (doc.TextBlock.Count > 0)
                            documents.Add(doc);

                        pages = new List<Page>();
                        hasDocumentTitle = false;
                    }

                    pages.Add(new Page());
                    pages.Last().Paragraphs.Add(line);
                    isNewParagraph = true;

                    continue;
                }

                //indicators for new paragraph
                DocumentType docType = DocumentTypeMapping.ParseUpperDocumentTypeLine(line);
                if (docType != DocumentType.Unknown)
                {
                    hasDocumentTitle = true;

                    docTypes.Add(docType);

                    if (paragraph.Length > 0)
                    {
                        pages.Last().Paragraphs.Add(paragraph.ToString());
                        paragraph.Clear();
                    }
                    pages.Last().Paragraphs.Add(line);
                    isNewParagraph = true;
                    continue;
                }

                bool isHandled = HandlePrefixes(new string[] { "CONG HOA XA HOI CHU NGHIA VIET NAM",
                    "Doc lap - Tu do - Hanh phuc",
                    "Kinh gui:" }, line, paragraph, pages.Last().Paragraphs,
                    isSeparatedLineForPrefix : true);

                if (isHandled)
                {
                    hasDocumentTitle = true;
                    isNewParagraph = true;
                    continue;
                }

                isHandled = HandlePrefixes(new string[] { "QUYET DINH:", "Noi nhan:" },
                    line, paragraph, pages.Last().Paragraphs,
                    isSeparatedLineForPrefix: true);

                if (isHandled)
                {
                    isNewParagraph = true;
                    continue;
                }

                isHandled = HandlePrefixes(new string[] { "KT.", "TL." },
                    line, paragraph, pages.Last().Paragraphs, isSeparatedLineForPrefix : false);

                if (isHandled)
                {
                    isNewParagraph = true;
                    continue;
                }

                //line is all uppercase, contains at least one letter
                if (line == line.ToUpper() && Regex.IsMatch(line, @"\p{L}"))
                {
                    if (paragraph.Length > 0)
                    {
                        pages.Last().Paragraphs.Add(paragraph.ToString());
                        paragraph.Clear();
                    }
                    pages.Last().Paragraphs.Add(line);
                    isNewParagraph = true;
                    continue;
                }

                //no indicators for new paragraph, append to current paragraph

                if (!isNewParagraph)
                {
                    paragraph.Append(" ");
                }

                paragraph.Append(line);

                //indicators for end of paragraph
                //line ends with punctuation except for +, -, , ( , /, \
                if (!Regex.IsMatch(line, @"[+\-,\(/\\]$") && Regex.IsMatch(line, @"[^\w\s]$"))
                {
                    pages.Last().Paragraphs.Add(paragraph.ToString());
                    paragraph.Clear();

                    isNewParagraph = true;
                    continue;
                }

                isNewParagraph = false;
            }

            //add any remaining paragraph
            if (paragraph.Length > 0)
                pages.Last().Paragraphs.Add(paragraph.ToString());

            Document lastDoc = new Document();
            if(docTypes.Count > 0)
            {
                lastDoc.DocTypes = docTypes;
                docTypes = new List<DocumentType>();
            }
            foreach (var page in pages)
                lastDoc.TextBlock.AddRange(page.Paragraphs);
            if (lastDoc.TextBlock.Count > 0)
                documents.Add(lastDoc);

            return new DocumentList(documents);
        }
    }
}
