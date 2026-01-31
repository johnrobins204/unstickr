using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;
using StoryFort.Models;

namespace StoryFort.Services;

public interface ICohereTutorService
{
    Task<string> GetSocraticPromptAsync(string prompt, Account account, bool useReasoningModel);
}

public class CohereTutorService : ICohereTutorService
{
    private readonly IHttpClientFactory _httpClientFactory;
    public CohereTutorService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string> GetSocraticPromptAsync(string prompt, Account account, bool useReasoningModel)
    {
        var client = _httpClientFactory.CreateClient("LLM");
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", account.CohereApiKey);
        var endpoint = useReasoningModel ? "v1/chat" : "v1/generate";
        object payload = useReasoningModel
            ? new { message = prompt, model = "command-r-plus" }
            : new { prompt = prompt, model = "command-light" };

        var response = await client.PostAsJsonAsync($"https://api.cohere.com/{endpoint}", payload);
        if (!response.IsSuccessStatusCode) return "AI service error.";

        var json = await response.Content.ReadAsStringAsync();
        try
        {
            if (useReasoningModel)
            {
                var chat = System.Text.Json.JsonSerializer.Deserialize<CohereChatResponse>(json);
                var msg = chat?.Message?.Content?.FirstOrDefault(c => c.Type == "text")?.Text;
                return msg ?? "No response.";
            }
            else
            {
                var gen = System.Text.Json.JsonSerializer.Deserialize<CohereGenerateResponse>(json);
                var text = gen?.Generations?.FirstOrDefault()?.Text;
                return text ?? "No response.";
            }
        }
        catch
        {
            return "AI service error (invalid response format).";
        }
    }

    // Old response class removed; now using direct JSON parsing.
}

