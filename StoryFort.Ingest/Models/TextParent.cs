using System.Collections.Generic;

namespace StoryFort.Ingest.Models
{
    /// <summary>
    /// Parent chunk for context injection (audit compliant)
    /// </summary>
    public class TextParent
    {
        public int Id { get; set; }
        public int PublicTextId { get; set; }
        public int? TextChapterId { get; set; }
        public string PlainText { get; set; }
        public int WordCount { get; set; }
        public int TokenCount { get; set; }
        public string StructuralPath { get; set; }
        public int? VerseNumber { get; set; }
        public ICollection<TextChunk> ChildChunks { get; set; }
    }
}