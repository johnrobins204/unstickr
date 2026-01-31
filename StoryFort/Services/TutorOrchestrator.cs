using System.Text.Json;
using StoryFort.Services;
using StoryFort.Models;
using System.Net.Http;
using System.Net.Http.Json;

namespace StoryFort.Services;

public class TutorOrchestrator
{
    private readonly ICohereTutorService _cohereTutorService;
    private readonly StoryState _storyState;
    private readonly SparkPromptStrategy _sparkStrategy;
    private readonly ReviewPromptStrategy _reviewStrategy;
    private readonly ISafeguardService _safeguardService;

    public TutorOrchestrator(ICohereTutorService cohereTutorService, StoryState storyState, SparkPromptStrategy sparkStrategy, ReviewPromptStrategy reviewStrategy, ISafeguardService safeguardService)
    {
        _cohereTutorService = cohereTutorService;
        _storyState = storyState;
        _sparkStrategy = sparkStrategy;
        _reviewStrategy = reviewStrategy;
        _safeguardService = safeguardService;
    }

    public async Task<string> RunSparkProtocolAsync(string modelName = "llama3")
    {
        // Guardrails
        var (isValid, error) = _safeguardService.ValidateSafeguards(_storyState);
        if (!isValid) return error!;

        _storyState.TutorSession.CurrentMode = TutorMode.SparkProtocol;
        return await _sparkStrategy.BuildPromptAsync(_storyState, modelName);
    }

    public async Task<string> RunReviewProtocolAsync(ReviewType type, string? modelName = null)
    {
        // Guardrails
        var (isValid, error) = _safeguardService.ValidateSafeguards(_storyState);
        if (!isValid) return error!;

        _storyState.TutorSession.CurrentMode = TutorMode.ReviewMode;
        return await _reviewStrategy.BuildPromptAsync(_storyState, modelName ?? "llama3");
    }

    // ...other orchestrator logic as needed...
}

