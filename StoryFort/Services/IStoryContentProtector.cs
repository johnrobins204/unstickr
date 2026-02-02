using Microsoft.AspNetCore.DataProtection;

namespace StoryFort.Services;

public interface IStoryContentProtector
{
    string Protect(string content);
    string Unprotect(string protectedContent);
}

public class StoryContentProtector : IStoryContentProtector
{
    private readonly IDataProtector _protector;

    public StoryContentProtector(IDataProtectionProvider provider)
    {
        // Unique purpose for story content
        _protector = provider.CreateProtector("StoryFort.Content.v1");
    }

    public string Protect(string content) => _protector.Protect(content ?? string.Empty);

    public string Unprotect(string protectedContent)
    {
        if (string.IsNullOrEmpty(protectedContent)) return string.Empty;
        try
        {
            return _protector.Unprotect(protectedContent);
        }
        catch
        {
            // Decryption might fail if key changed or data is old/plaintext
            return protectedContent; 
        }
    }
}

public static class StoryEncryptionProvider
{
    public static IStoryContentProtector? Protector { get; set; }
}

