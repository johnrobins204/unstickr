using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StoryFort.Ingest.Services
{
    /// <summary>
    /// Implementation of ISemanticChunker using token count (audit compliant)
    /// </summary>
    public class SemanticChunker : ISemanticChunker
    {
        public List<TextChunkDto> ChunkParagraphs(List<string> paragraphs, int maxTokens = 380, int overlapTokens = 50)
        {
            var chunks = new List<TextChunkDto>();
            var currentChunk = new StringBuilder();
            var currentTokens = 0;
            var startParaIndex = 0;
            var overlapBuffer = new Queue<string>();

            for (int i = 0; i < paragraphs.Count; i++)
            {
                var para = paragraphs[i];
                int tokens = EstimateTokenCount(para);

                if (currentTokens + tokens > maxTokens && currentTokens > 0)
                {
                    // Emit current chunk
                    chunks.Add(new TextChunkDto
                    {
                        PlainText = currentChunk.ToString().Trim(),
                        WordCount = currentChunk.ToString().Split(' ').Length,
                        TokenCount = currentTokens,
                        StartParagraphIndex = startParaIndex,
                        EndParagraphIndex = i - 1
                    });

                    // Start new chunk with overlap
                    currentChunk.Clear();
                    currentChunk.Append(string.Join(" ", overlapBuffer));
                    currentTokens = overlapBuffer.Sum(p => EstimateTokenCount(p));
                    startParaIndex = System.Math.Max(0, i - overlapBuffer.Count);
                }

                currentChunk.Append(para).Append(" ");
                currentTokens += tokens;

                // Maintain overlap buffer
                overlapBuffer.Enqueue(para);
                while (overlapBuffer.Sum(p => EstimateTokenCount(p)) > overlapTokens)
                {
                    overlapBuffer.Dequeue();
                }
            }

            // Emit final chunk
            if (currentTokens > 0)
            {
                chunks.Add(new TextChunkDto
                {
                    PlainText = currentChunk.ToString().Trim(),
                    WordCount = currentChunk.ToString().Split(' ').Length,
                    TokenCount = currentTokens,
                    StartParagraphIndex = startParaIndex,
                    EndParagraphIndex = paragraphs.Count - 1
                });
            }

            return chunks;
        }

        // Placeholder: Replace with Cohere tokenizer for production
        private int EstimateTokenCount(string text)
        {
            // Rough estimate: 1.33 tokens per word
            return (int)(text.Split(' ').Length * 1.33);
        }
    }
}