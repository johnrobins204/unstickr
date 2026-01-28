using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
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
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            try
            {
                if (useReasoningModel)
                {
                    // v1/chat: { message: { content: [ { type: "text", text: "..." } ] } }
                    var chatObj = System.Text.Json.JsonDocument.Parse(json);
                    var message = chatObj.RootElement.GetProperty("message");
                    var contentArr = message.GetProperty("content");
                    foreach (var item in contentArr.EnumerateArray())
                    {
                        if (item.TryGetProperty("text", out var textProp))
                            return textProp.GetString() ?? "No response.";
                    }
                    return "No response.";
                }
                else
                {
                    // v1/generate: { generations: [ { text: "..." } ] }
                    var genObj = System.Text.Json.JsonDocument.Parse(json);
                    var generations = genObj.RootElement.GetProperty("generations");
                    foreach (var item in generations.EnumerateArray())
                    {
                        if (item.TryGetProperty("text", out var textProp))
                            return textProp.GetString() ?? "No response.";
                    }
                    return "No response.";
                }
            }
            catch
            {
                return "AI service error (invalid response format).";
            }
        }
        return "AI service error.";
    }

    // Old response class removed; now using direct JSON parsing.
}

