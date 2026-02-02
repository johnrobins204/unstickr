namespace StoryFort.Services;

public class SparkPromptOptions
{
    public string SystemPrompt { get; set; } = string.Empty;
    public string ReadyToWriteJson { get; set; } = "{\"status\": \"READY_TO_WRITE\"}";
}
