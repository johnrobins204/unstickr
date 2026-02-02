namespace StoryFort.Ingest.Models
{
    /// <summary>
    /// Child chunk for vector search (audit compliant)
    /// </summary>
    public class TextChunk
    {
        public int Id { get; set; }
        public int PublicTextId { get; set; }
        public int? TextChapterId { get; set; }
        public int? ParentChunkId { get; set; }
        public string PlainText { get; set; }
        public int WordCount { get; set; }
        public int TokenCount { get; set; }
        public int StartParagraphIndex { get; set; }
        public int EndParagraphIndex { get; set; }
        public int ChunkIndexInChapter { get; set; }
        public string SourceAnchor { get; set; }
    }
}