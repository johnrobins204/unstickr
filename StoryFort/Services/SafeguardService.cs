using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using StoryFort.Models;

namespace StoryFort.Services;

public class SafeguardService : ISafeguardService
{
    private readonly SafeguardOptions _options;

    public SafeguardService(IOptions<SafeguardOptions>? options = null)
    {
        _options = options?.Value ?? new SafeguardOptions();
    }

    public (bool IsValid, string? Error) ValidateSafeguards(StoryContext context)
    {
        if (string.IsNullOrWhiteSpace(context.Account?.ProtectedCohereApiKey))
        {
            return (false, "AI Service Error: No API Key configured. Please contact your supervisor.");
        }

        var content = context.Content ?? "";

        // Prompt Injection Defense (OWASP LLM01)
        if (!string.IsNullOrEmpty(_options.PromptInjectionPattern) && 
            Regex.IsMatch(content, _options.PromptInjectionPattern, RegexOptions.IgnoreCase))
        {
            return (false, "Safety Guardrail: Your input contains patterns associated with prompt injection. Please revise.");
        }

        // Basic PII (email) check
        if (!string.IsNullOrEmpty(_options.PiiPattern) && 
            Regex.IsMatch(content, _options.PiiPattern, RegexOptions.IgnoreCase))
        {
            return (false, "Safety Guardrail: Potential personal information (email) detected. Please remove identifiable info before using the Tutor.");
        }

        // Banned Words / Inappropriate Content Filter (G-02)
        if (_options.BannedWordsPatterns != null && _options.BannedWordsPatterns.Any())
        {
            foreach (var pattern in _options.BannedWordsPatterns)
            {
                if (!string.IsNullOrEmpty(pattern) && Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase))
                {
                    return (false, "Please keep your writing appropriate for school.");
                }
            }
        }

        return (true, null);
    }
}
