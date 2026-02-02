namespace StoryFort.Ingest.Services
{
    /// <summary>
    /// Removes "Project Gutenberg" trademark for commercial use (audit compliant)
    /// </summary>
    public interface ITrademarkStripperService
    {
        string StripGutenbergTrademarks(string text);
        EpubMetadata CleanMetadata(EpubMetadata metadata);
    }

    // Placeholder for EPUB metadata type
    public class EpubMetadata
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string License { get; set; }
        public string Source { get; set; }
    }
}