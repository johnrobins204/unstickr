namespace StoryFort.Services;

public interface ISafeguardService
{
    (bool IsValid, string? Error) ValidateSafeguards(StoryState storyState);
}
