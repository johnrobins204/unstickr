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
        // Guardrails
        var (isValid, error) = ValidateSafeguards();
        if (!isValid) return error!;

        _storyState.TutorSession.CurrentMode = TutorMode.SparkProtocol;
        return await _sparkStrategy.BuildPromptAsync(_storyState, modelName);
    }

    public async Task<string> RunReviewProtocolAsync(ReviewType type, string? modelName = null)
    {
        // Guardrails
        var (isValid, error) = ValidateSafeguards();
        if (!isValid) return error!;

        _storyState.TutorSession.CurrentMode = TutorMode.ReviewMode;
        return await _reviewStrategy.BuildPromptAsync(_storyState, modelName ?? "llama3");
    }

    /// <summary>
    /// Implements "Defense in Depth" for AI Safety (Prompt Injection + PII)
    /// </summary>
    private (bool IsValid, string? Error) ValidateSafeguards()
    {
        // 1. Account Configuration Check
        if (string.IsNullOrWhiteSpace(_storyState.Account?.CohereApiKey))
        {
            return (false, "AI Service Error: No API Key configured. Please contact your supervisor.");
        }

        var content = _storyState.Content ?? "";

        // 2. Prompt Injection Defense (OWASP LLM01)
        // Check for attempts to override system instructions
        if (System.Text.RegularExpressions.Regex.IsMatch(content, @"(ignore|disregard)\s+(all\s+)?previous\s+instructions", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
        {
            return (false, "Safety Guardrail: Your input contains patterns associated with prompt injection. Please revise.");
        }

        // 3. PII Firewall (Pre-flight)
        // Basic check for things that look like emails before sending to external API
        if (System.Text.RegularExpressions.Regex.IsMatch(content, @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
        {
            return (false, "Safety Guardrail: Potential personal information (email) detected. Please remove identifiable info before using the Tutor.");
        }

        return (true, null);
    }

    // ...other orchestrator logic as needed...
}
