using System.Threading.Tasks;

namespace StoryFort.Ingest.Services
{
    /// <summary>
    /// Corrects OCR errors using LLM (audit compliant)
    /// </summary>
    public interface IOcrCorrectionService
    {
        Task<string> CorrectOcrErrorsAsync(string rawOcrText);
        double EstimateOcrQuality(string text);
    }
}