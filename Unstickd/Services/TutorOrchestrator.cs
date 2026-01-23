using System.Text.Json;
using Unstickd.Services;
using Unstickd.Models;
using System.Net.Http;
using System.Net.Http.Json;

namespace Unstickd.Services;

public class TutorOrchestrator
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly StoryState _storyState;

    public TutorOrchestrator(IHttpClientFactory httpClientFactory, StoryState storyState)
    {
        _httpClientFactory = httpClientFactory;
        _storyState = storyState;
    }

    public async Task<string> RunSparkProtocolAsync(string modelName = "llama3")
    {
        var session = _storyState.TutorSession;
        session.CurrentMode = TutorMode.SparkProtocol;

        // Step 1: Sensory Question
        if (session.History.Count == 0)
        {
            var prompt = "Ask the user a sensory question to start their story. Only ask a question, do not generate prose.";
            return await SendPromptAsync(prompt, modelName);
        }
        // Step 2: Attribute Narrowing
        else if (session.History.Count == 1)
        {
            var prompt = "Based on the user's answer, ask an attribute narrowing question. Only ask a question.";
            return await SendPromptAsync(prompt, modelName);
        }
        // Step 3: Scenario Prompt
        else if (session.History.Count == 2)
        {
            var prompt = "Based on previous answers, ask a scenario question to help the user imagine a situation. Only ask a question.";
            return await SendPromptAsync(prompt, modelName);
        }
        // Step 4: Output READY_TO_WRITE
        else
        {
            var prompt = "If the user is ready, output JSON: {\"status\": \"READY_TO_WRITE\"}. Otherwise, ask a final clarifying question.";
            var response = await SendPromptAsync(prompt, modelName);
            // Try to parse JSON status
            try
            {
                var json = JsonDocument.Parse(response);
                if (json.RootElement.TryGetProperty("status", out var status) && status.GetString() == "READY_TO_WRITE")
                {
                    session.LastJsonStatus = "READY_TO_WRITE";
                }
            }
            catch { /* Not JSON, ignore */ }
            return response;
        }
    }

    private async Task<string> SendPromptAsync(string prompt, string modelName)
    {
        var client = _httpClientFactory.CreateClient("LLM");
        var context = $"The user is writing a story titled '{_storyState.Title}'. Content snippet: '{_storyState.Content}'.";
        var fullPrompt = $"System: You are a Socratic writing coach. Context: {context}. User: {prompt}";
        var payload = new { model = modelName, prompt = fullPrompt, stream = false };
        var response = await client.PostAsJsonAsync("api/generate", payload);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<LlamaResponse>();
            if (result != null)
            {
                _storyState.TutorSession.History.Add(result.response);
                return result.response;
            }
        }
        return "AI service error.";
    }

    private class LlamaResponse
    {
        public string response { get; set; } = "";
    }

    // ...moved to Unstickd.Models.TutorEnums.cs...

    public async Task<string> RunReviewProtocolAsync(ReviewType type, string? modelName = null)
    {
        var genre = _storyState.Genre ?? "General";
        var content = _storyState.Content ?? string.Empty;
        var snippet = content.Length > 500 ? content.Substring(content.Length - 500) : content;
        string prompt;
        switch (type)
        {
            case ReviewType.Orthographic:
                prompt = $"You are a teacher. Find one grammar rule broken in this text: '{snippet}'. Explain the rule using the 'Sandwich Method' (Praise-Correct-Praise). Do not rewrite the text.";
                break;
            case ReviewType.Style:
                var author = GetAuthorForGenre(genre);
                prompt = $"You are a literary critic. The genre is '{genre}'. Identify one weak verb or adjective in: '{snippet}'. Suggest an alternative that a famous author of this genre (like {author}) might use. Quote a real sentence from a public domain book in this genre as an example.";
                break;
            default:
                prompt = "Provide feedback on the following text.";
                break;
        }
        return await SendPromptAsync(prompt, modelName ?? "llama3");
    }

    private string GetAuthorForGenre(string genre)
    {
        var map = new Dictionary<string, string[]> {
            { "Mystery", new[] { "Arthur Conan Doyle", "Agatha Christie" } },
            { "Fantasy", new[] { "J.R.R. Tolkien", "Lewis Carroll" } },
            { "Sci-Fi", new[] { "H.G. Wells", "Jules Verne" } },
            { "Adventure", new[] { "Robert Louis Stevenson", "Jack London" } },
            { "General", new[] { "Mark Twain", "Louisa May Alcott" } }
        };
        if (map.TryGetValue(genre, out var authors))
            return authors[0];
        return "Mark Twain";
    }
}
