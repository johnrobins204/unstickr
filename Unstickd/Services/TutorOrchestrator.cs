using System.Text.Json;
using Unstickd.Services;
using Unstickd.Models;
using System.Net.Http;
using System.Net.Http.Json;

namespace Unstickd.Services;

public class TutorOrchestrator
{
    private readonly ICohereTutorService _cohereTutorService;
    private readonly StoryState _storyState;
    private readonly IPromptStrategy _sparkStrategy;
    private readonly IPromptStrategy _reviewStrategy;

    public TutorOrchestrator(ICohereTutorService cohereTutorService, StoryState storyState)
    {
        _cohereTutorService = cohereTutorService;
        _storyState = storyState;
        _sparkStrategy = new SparkPromptStrategy(_cohereTutorService);
        _reviewStrategy = new ReviewPromptStrategy(_cohereTutorService);
    }

    public async Task<string> RunSparkProtocolAsync(string modelName = "llama3")
    {
        _storyState.TutorSession.CurrentMode = TutorMode.SparkProtocol;
        return await _sparkStrategy.BuildPromptAsync(_storyState, modelName);
    }

    public async Task<string> RunReviewProtocolAsync(ReviewType type, string? modelName = null)
    {
        _storyState.TutorSession.CurrentMode = TutorMode.ReviewMode;
        return await _reviewStrategy.BuildPromptAsync(_storyState, modelName ?? "llama3");
    }

    // ...other orchestrator logic as needed...
}
