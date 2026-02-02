using System;
using System.IO;
using System.Threading.Tasks;
using StoryFort.Ingest.Services;

namespace StoryFort.Ingest
{
    public class IngestAlice
    {
        public static async Task RunAsync()
        {
            // Alice in Wonderland Gutenberg ID: 11
            int gutenbergId = 11;
            string outputPath = "Data/epub_cache/alice.epub";
            string sourceUrl = "https://www.gutenberg.org/ebooks/11";

            var mirrorService = new GutenbergMirrorService();
            var parser = new EpubParser();
            var chunker = new SemanticChunker();

            // Download EPUB via rsync mirror (single polite request)
            if (!File.Exists(outputPath))
            {
                Console.WriteLine("Downloading Alice in Wonderland EPUB via mirror...");
                await mirrorService.SyncFromMirrorAsync(gutenbergId, outputPath);
                Console.WriteLine("Download complete.");
            }
            else
            {
                Console.WriteLine("EPUB already cached.");
            }

            // Parse EPUB
            var parsed = await parser.ParseAsync(outputPath);
            Console.WriteLine($"Parsed: {parsed.Title} by {parsed.Author}");

            // Chunk first chapter only (slow, polite)
            if (parsed.Chapters.Count > 0)
            {
                var chapter = parsed.Chapters[0];
                var chunks = chunker.ChunkParagraphs(chapter.Paragraphs);
                Console.WriteLine($"First chapter: {chapter.Title}, {chunks.Count} chunks (token-safe)");
                foreach (var chunk in chunks)
                {
                    Console.WriteLine($"Chunk: {chunk.TokenCount} tokens, {chunk.WordCount} words");
                }
            }
            else
            {
                Console.WriteLine("No chapters found in EPUB.");
            }
        }
    }
}