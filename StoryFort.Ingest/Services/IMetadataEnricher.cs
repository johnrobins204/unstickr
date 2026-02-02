using System.Collections.Generic;
using System.Threading.Tasks;

namespace StoryFort.Ingest.Services
{
    /// <summary>
    /// Extracts structural metadata (verse numbers, chapters) for sacred texts (audit compliant)
    /// </summary>
    public interface IMetadataEnricher
    {
        Task<StructuralMetadata> ExtractStructureAsync(ParsedEpubBook book);
    }

    public class StructuralMetadata
    {
        public string HierarchyType { get; set; }
        public Dictionary<int, string> ChapterTitles { get; set; }
        public Dictionary<int, int> VerseNumbers { get; set; }
        public List<string> StructuralPaths { get; set; }
    }

    // Placeholder for ParsedEpubBook type
    public class ParsedEpubBook
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public List<ParsedChapter> Chapters { get; set; }
    }

    public class ParsedChapter
    {
        public string Title { get; set; }
        public string Anchor { get; set; }
        public List<string> Paragraphs { get; set; }
    }
}