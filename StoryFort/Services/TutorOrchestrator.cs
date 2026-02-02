using System.Text.Json;
using StoryFort.Services;
using StoryFort.Models;
using System.Net.Http;
using System.Net.Http.Json;

namespace StoryFort.Services;

public class TutorOrchestrator
{
    private readonly ICohereTutorService _cohereTutorService;
    private readonly StoryContext _storyContext;
    private readonly IPromptStrategy _sparkStrategy;
    private readonly IPromptStrategy _reviewStrategy;
    private readonly ISafeguardService _safeguardService;
    private readonly TutorSessionService _tutorSessionService;

    public TutorOrchestrator(ICohereTutorService cohereTutorService, StoryContext storyContext, IPromptStrategy sparkStrategy, IPromptStrategy reviewStrategy, ISafeguardService safeguardService, TutorSessionService tutorSessionService)
    {
        _cohereTutorService = cohereTutorService;
        _storyContext = storyContext;
        _sparkStrategy = sparkStrategy;
        _reviewStrategy = reviewStrategy;
        _safeguardService = safeguardService;
        _tutorSessionService = tutorSessionService;
    }

    public async Task<string> RunSparkProtocolAsync(string modelName = "llama3")
    {
        // Guardrails
        var (isValid, error) = _safeguardService.ValidateSafeguards(_storyContext);
        if (!isValid) return error!;

        _tutorSessionService.SetMode(TutorMode.SparkProtocol);
        return await _sparkStrategy.BuildPromptAsync(_storyContext, modelName);
    }

    public async Task<string> RunReviewProtocolAsync(ReviewType type, string? modelName = null)
    {
        // Guardrails
        var (isValid, error) = _safeguardService.ValidateSafeguards(_storyContext);
        if (!isValid) return error!;

        _tutorSessionService.SetMode(TutorMode.ReviewMode);
        return await _reviewStrategy.BuildPromptAsync(_storyContext, modelName ?? "llama3");
    }

    // ...other orchestrator logic as needed...
}

