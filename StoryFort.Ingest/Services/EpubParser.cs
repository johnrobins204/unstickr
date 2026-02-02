using System.Collections.Generic;
using System.Threading.Tasks;
using VersOne.Epub;
using AngleSharp.Html.Parser;

namespace StoryFort.Ingest.Services
{
    /// <summary>
    /// Implementation of IEpubParser using VersOne.Epub and AngleSharp (audit compliant)
    /// </summary>
    public class EpubParser : IEpubParser
    {
        public async Task<ParsedEpubBook> ParseAsync(string epubPath)
        {
            var book = await EpubReader.ReadBookAsync(epubPath);
            var parsedBook = new ParsedEpubBook
            {
                Title = book.Title,
                Author = book.Author,
                Chapters = new List<ParsedChapter>()
            };

            var htmlParser = new HtmlParser();
            foreach (var chapter in book.ReadingOrder)
            {
                var htmlContent = chapter.Content ?? string.Empty;
                var document = htmlParser.ParseDocument(htmlContent);
                var paragraphs = new List<string>();
                foreach (var p in document.QuerySelectorAll("p"))
                {
                    paragraphs.Add(p.TextContent.Trim());
                }
                parsedBook.Chapters.Add(new ParsedChapter
                {
                    Title = chapter.FileName ?? "Chapter",
                    Anchor = chapter.FileName,
                    Paragraphs = paragraphs
                });
            }
            return parsedBook;
        }
    }
}