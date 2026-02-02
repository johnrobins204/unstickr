using System.Threading.Tasks;

namespace StoryFort.Ingest.Services
{
    /// <summary>
    /// Generates embeddings for batch of texts using Cohere v4 (audit compliant)
    /// </summary>
    public interface ICohereEmbedService
    {
        Task<float[][]> EmbedBatchAsync(
            string[] texts,
            string model = "embed-v4.0",
            int dimensions = 1024
        );
    }
}