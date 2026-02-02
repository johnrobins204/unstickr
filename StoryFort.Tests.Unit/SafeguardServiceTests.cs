using Microsoft.Extensions.Options;
using Moq;
using StoryFort.Models;
using StoryFort.Services;
using Xunit;

namespace StoryFort.Tests.Unit;

public class SafeguardServiceTests
{
    [Fact]
    public void ValidateSafeguards_UsesCustomRegex()
    {
        // Arrange
        var options = new SafeguardOptions
        {
            PromptInjectionPattern = "BAD_WORD",
            PiiPattern = "FORBIDDEN"
        };
        var mockOptions = new Mock<IOptions<SafeguardOptions>>();
        mockOptions.Setup(o => o.Value).Returns(options);

        var svc = new SafeguardService(mockOptions.Object);
        var context = new StoryContext { Account = new Account { ProtectedCohereApiKey = "protected-key" } };
        
        // Act & Assert
        context.Content = "This is a BAD_WORD in the story.";
        var (isValid1, error1) = svc.ValidateSafeguards(context);
        Assert.False(isValid1);
        Assert.Contains("prompt injection", error1, System.StringComparison.OrdinalIgnoreCase);

        context.Content = "This is FORBIDDEN text.";
        var (isValid2, error2) = svc.ValidateSafeguards(context);
        Assert.False(isValid2);
        Assert.Contains("personal information", error2, System.StringComparison.OrdinalIgnoreCase);
    }
}
