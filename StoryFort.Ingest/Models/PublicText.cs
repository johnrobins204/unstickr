using System.Collections.Generic;

namespace StoryFort.Ingest.Models
{
    public class PublicText
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Genre { get; set; }
        public string SourceUrl { get; set; }
        public List<TextChapter> Chapters { get; set; }

        // Audit compliance fields
        public bool IsOcrSource { get; set; }
        public bool HasStructuralMetadata { get; set; }
        public double OcrConfidenceScore { get; set; }
        public bool TrademarkStripped { get; set; }
    }
}