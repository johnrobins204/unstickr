namespace StoryFort.Ingest.Services
{
    /// <summary>
    /// Implementation of ITrademarkStripperService (audit compliant)
    /// </summary>
    public class TrademarkStripperService : ITrademarkStripperService
    {
        public string StripGutenbergTrademarks(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            // Remove "Project Gutenberg" and license headers
            text = text.Replace("Project Gutenberg", "Public Domain Archive");
            // Remove common license header patterns
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\*\*\* START OF THIS PROJECT GUTENBERG EBOOK.*?\*\*\* END OF THIS PROJECT GUTENBERG EBOOK.*?", string.Empty, System.Text.RegularExpressions.RegexOptions.Singleline);
            return text;
        }

        public EpubMetadata CleanMetadata(EpubMetadata metadata)
        {
            if (metadata == null) return null;
            metadata.Source = "Public Domain Archive";
            metadata.License = "Public Domain";
            return metadata;
        }
    }
}