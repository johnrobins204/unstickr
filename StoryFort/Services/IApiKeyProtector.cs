namespace StoryFort.Services;

public interface IApiKeyProtector
{
    string Protect(string plain);
    string Unprotect(string protectedValue);
}
