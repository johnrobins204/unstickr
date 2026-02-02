namespace StoryFort.Services;

public class SafeguardOptions
{
    public string PromptInjectionPattern { get; set; } = @"(ignore|disregard)\s+(all\s+)?previous\s+instructions";
    public string PiiPattern { get; set; } = @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}";
    
    /// <summary>
    /// List of regex patterns for inappropriate content filtering.
    /// Each pattern is checked against user input before AI processing.
    /// </summary>
    public List<string> BannedWordsPatterns { get; set; } = new();
}
