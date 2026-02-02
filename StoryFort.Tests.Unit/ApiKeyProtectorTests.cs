using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using StoryFort.Services;
using Xunit;

namespace StoryFort.Tests.Unit;

public class ApiKeyProtectorTests
{
    [Fact]
    public void ProtectAndUnprotect_RoundTrips()
    {
        var services = new ServiceCollection();
        services.AddDataProtection();
        var provider = services.BuildServiceProvider();
        var dp = provider.GetRequiredService<IDataProtectionProvider>();
        var protector = new ApiKeyProtector(dp);

        var secret = "sk-test-123";
        var p = protector.Protect(secret);
        p.Should().NotBeNullOrEmpty();

        var un = protector.Unprotect(p);
        un.Should().Be(secret);
    }
}
