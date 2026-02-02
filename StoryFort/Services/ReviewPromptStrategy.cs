using System.Threading.Tasks;
using StoryFort.Models;

namespace StoryFort.Services;

public class ReviewPromptStrategy : IPromptStrategy
{
    private readonly ICohereTutorService _cohereTutorService;
    private readonly TutorSessionService _tutorSessionService;
    public ReviewPromptStrategy(ICohereTutorService cohereTutorService, TutorSessionService tutorSessionService)
    {
        _cohereTutorService = cohereTutorService;
        _tutorSessionService = tutorSessionService;
    }

    public async Task<string> BuildPromptAsync(StoryContext context, string modelName)
    {
        var genre = context.Genre ?? "General";
        var content = context.Content ?? string.Empty;
        var snippet = content.Length > 500 ? content.Substring(content.Length - 500) : content;
        string prompt = $"You are a teacher. Find one grammar rule broken in this text: '{snippet}'. Explain the rule using the 'Sandwich Method' (Praise-Correct-Praise). Do not rewrite the text.";
        var account = context.Account ?? new Account();
        var response = await _cohereTutorService.GetSocraticPromptAsync(prompt, account, account.UseReasoningModel);
        _tutorSessionService.AddHistory(response);
        return response;
    }
}

