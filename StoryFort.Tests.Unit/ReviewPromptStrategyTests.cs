using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using StoryFort.Models;
using StoryFort.Services;
using Xunit;

namespace StoryFort.Tests.Unit;

public class ReviewPromptStrategyTests
{
    [Fact]
    public async Task BuildPromptAsync_CallsCohereAndAddsHistory()
    {
        var mockCohere = new Mock<ICohereTutorService>();
        var tutorSession = new TutorSessionService();

        mockCohere.Setup(c => c.GetSocraticPromptAsync(It.Is<string>(s => s.Contains("Find one grammar rule")), It.IsAny<Account>(), It.IsAny<bool>()))
            .ReturnsAsync("Feedback from AI");

        var strategy = new ReviewPromptStrategy(mockCohere.Object, tutorSession);

        var context = new StoryContext { Content = "This is some bad grammar text." , Genre = "General", Account = new Account() };

        var result = await strategy.BuildPromptAsync(context, "model");

        result.Should().Be("Feedback from AI");
        tutorSession.HistoryCount.Should().BeGreaterThan(0);
        tutorSession.Session.History.Should().Contain("Feedback from AI");
    }
}

