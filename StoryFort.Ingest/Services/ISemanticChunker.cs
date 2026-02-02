using System.Collections.Generic;

namespace StoryFort.Ingest.Services
{
    /// <summary>
    /// Chunks paragraphs into RAG-optimized segments using token count (audit compliant)
    /// </summary>
    public interface ISemanticChunker
    {
        List<TextChunkDto> ChunkParagraphs(
            List<string> paragraphs,
            int maxTokens = 380,
            int overlapTokens = 50
        );
    }

    public class TextChunkDto
    {
        public string PlainText { get; set; }
        public int WordCount { get; set; }
        public int TokenCount { get; set; }
        public int StartParagraphIndex { get; set; }
        public int EndParagraphIndex { get; set; }
    }
}