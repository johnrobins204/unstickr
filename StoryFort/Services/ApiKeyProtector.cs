using Microsoft.AspNetCore.DataProtection;

namespace StoryFort.Services;

public class ApiKeyProtector : IApiKeyProtector
{
	private readonly IDataProtector _protector;

	public ApiKeyProtector(IDataProtectionProvider provider)
	{
		_protector = provider.CreateProtector("StoryFort.ApiKeys.v1");
	}

	public string Protect(string plain) => _protector.Protect(plain ?? string.Empty);

	public string Unprotect(string protectedValue)
	{
		if (string.IsNullOrEmpty(protectedValue)) return string.Empty;
		try
		{
			return _protector.Unprotect(protectedValue);
		}
		catch
		{
			return string.Empty;
		}
	}
}