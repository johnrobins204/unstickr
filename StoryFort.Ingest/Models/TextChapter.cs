using System.Collections.Generic;

namespace StoryFort.Ingest.Models
{
    public class TextChapter
    {
        public int Id { get; set; }
        public int PublicTextId { get; set; }
        public string Title { get; set; }
        public int ChapterNumber { get; set; }
        public string ChapterAnchor { get; set; }
        public string EpubItemId { get; set; }
        public int WordCount { get; set; }
        public int StartParagraphIndex { get; set; }
        public int EndParagraphIndex { get; set; }
        public List<TextChunk> Chunks { get; set; }
        public List<TextParent> Parents { get; set; }
    }
}