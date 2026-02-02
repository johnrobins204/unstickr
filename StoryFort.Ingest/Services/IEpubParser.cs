using System.Threading.Tasks;

namespace StoryFort.Ingest.Services
{
    public interface IEpubParser
    {
        Task<ParsedEpubBook> ParseAsync(string epubPath);
    }
}