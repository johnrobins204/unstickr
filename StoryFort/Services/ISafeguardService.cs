using StoryFort.Models;

namespace StoryFort.Services;

public interface ISafeguardService
{
    (bool IsValid, string? Error) ValidateSafeguards(StoryContext context);
}
