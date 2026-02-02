using System.Threading.Tasks;

namespace StoryFort.Ingest.Services
{
    /// <summary>
    /// Downloads EPUBs from Project Gutenberg mirrors via rsync (audit compliant)
    /// </summary>
    public interface IGutenbergMirrorService
    {
        Task<string> SyncFromMirrorAsync(int gutenbergId, string outputPath);
        int? ExtractGutenbergId(string url);
    }
}