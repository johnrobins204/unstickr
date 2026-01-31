using System.Text.RegularExpressions;

namespace StoryFort.Services;

public class SafeguardService : ISafeguardService
{
    public (bool IsValid, string? Error) ValidateSafeguards(StoryState storyState)
    {
        if (string.IsNullOrWhiteSpace(storyState.Account?.CohereApiKey))
        {
            return (false, "AI Service Error: No API Key configured. Please contact your supervisor.");
        }

        var content = storyState.Content ?? "";

        // Prompt Injection Defense (OWASP LLM01)
        if (Regex.IsMatch(content, @"(ignore|disregard)\s+(all\s+)?previous\s+instructions", RegexOptions.IgnoreCase))
        {
            return (false, "Safety Guardrail: Your input contains patterns associated with prompt injection. Please revise.");
        }

        // Basic PII (email) check
        if (Regex.IsMatch(content, @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}", RegexOptions.IgnoreCase))
        {
            return (false, "Safety Guardrail: Potential personal information (email) detected. Please remove identifiable info before using the Tutor.");
        }

        return (true, null);
    }
}
