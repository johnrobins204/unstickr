using System.Threading.Tasks;
using StoryFort.Models;

namespace StoryFort.Services;

public class ReviewPromptStrategy : IPromptStrategy
{
    private readonly ICohereTutorService _cohereTutorService;
    public ReviewPromptStrategy(ICohereTutorService cohereTutorService)
    {
        _cohereTutorService = cohereTutorService;
    }

    public async Task<string> BuildPromptAsync(StoryState state, string modelName)
    {
        var genre = state.Genre ?? "General";
        var content = state.Content ?? string.Empty;
        var snippet = content.Length > 500 ? content.Substring(content.Length - 500) : content;
        string prompt = $"You are a teacher. Find one grammar rule broken in this text: '{snippet}'. Explain the rule using the 'Sandwich Method' (Praise-Correct-Praise). Do not rewrite the text.";
        var account = state.Account ?? new Account();
        var response = await _cohereTutorService.GetSocraticPromptAsync(prompt, account, account.UseReasoningModel);
        state.TutorSession.History.Add(response);
        return response;
    }
}

