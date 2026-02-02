using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using StoryFort.Models;
using StoryFort.Services;
using Xunit;

namespace StoryFort.Tests.Unit;

public class SparkPromptStrategyTests
{
    private readonly Mock<ICohereTutorService> _mockCohere = new();
    private readonly Mock<IOptionsSnapshot<SparkPromptOptions>> _mockOptions = new();
    private readonly Mock<TutorSessionService> _mockSessionService = new();
    private readonly Mock<PromptRepository> _mockRepo;
    private readonly Mock<IPromptService> _mockPromptService = new();

    public SparkPromptStrategyTests()
    {
        // Mock PromptRepository is a bit tricky since it needs IHostEnvironment
        var mockEnv = new Mock<Microsoft.Extensions.Hosting.IHostEnvironment>();
        _mockRepo = new Mock<PromptRepository>(mockEnv.Object);

        _mockOptions.Setup(o => o.Value).Returns(new SparkPromptOptions());
    }

    [Fact]
    public async Task BuildPromptAsync_FirstTurn_CallsCohereWithSensoryPrompt()
    {
        // Arrange
        var strategy = new SparkPromptStrategy(
            _mockCohere.Object, 
            _mockOptions.Object, 
            _mockSessionService.Object, 
            _mockRepo.Object, 
            _mockPromptService.Object);

        var context = new StoryContext();
        context.Account = new Account { ProtectedCohereApiKey = "protected-key" };

        _mockSessionService.Setup(s => s.HistoryCount).Returns(0);
        _mockCohere.Setup(c => c.GetSocraticPromptAsync(It.IsAny<string>(), It.IsAny<Account>(), It.IsAny<bool>()))
            .ReturnsAsync("Sensory Question?");
        
        _mockSessionService.Setup(s => s.Session).Returns(new TutorSession());

        // Act
        var result = await strategy.BuildPromptAsync(context, "test-model");

        // Assert
        Assert.Equal("Sensory Question?", result);
        _mockCohere.Verify(c => c.GetSocraticPromptAsync(It.Is<string>(s => s.Contains("sensory")), It.IsAny<Account>(), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task BuildPromptAsync_UsesPromptService()
    {
        // Arrange
        var strategy = new SparkPromptStrategy(
            _mockCohere.Object, 
            _mockOptions.Object, 
            _mockSessionService.Object, 
            _mockRepo.Object, 
            _mockPromptService.Object);

        var context = new StoryContext();
        context.Account = new Account { ProtectedCohereApiKey = "protected-key" };

        _mockSessionService.Setup(s => s.HistoryCount).Returns(0);
        _mockPromptService.Setup(p => p.FormatTemplate(It.IsAny<string>(), It.IsAny<IDictionary<string, string?>>()))
            .Returns("Custom Prompt Template");

        _mockCohere.Setup(c => c.GetSocraticPromptAsync(It.IsAny<string>(), It.IsAny<Account>(), It.IsAny<bool>()))
            .ReturnsAsync("Response");
        
        _mockSessionService.Setup(s => s.Session).Returns(new TutorSession());

        // Act
        await strategy.BuildPromptAsync(context, "test-model");

        // Assert
        _mockCohere.Verify(c => c.GetSocraticPromptAsync(It.Is<string>(s => s.Contains("Custom Prompt Template")), It.IsAny<Account>(), It.IsAny<bool>()), Times.Once);
    }
}
