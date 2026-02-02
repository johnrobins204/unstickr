using System.Text.Json;
using FluentAssertions;
using StoryFort.Models;
using Xunit;

namespace StoryFort.Tests.Unit;

public class ModelValidationTests
{
    [Fact]
    public void Story_MetadataMap_SerializesAndDeserializes()
    {
        var story = new Story { Title = "T", Content = "<p>Hi</p>" };
        var json = JsonSerializer.Serialize(story);
        var round = JsonSerializer.Deserialize<Story>(json);
        round.Should().NotBeNull();
        round!.Title.Should().Be("T");
    }

    [Fact]
    public void ThemePreference_DefaultValues_AreValid()
    {
        var pref = new ThemePreference();
        pref.FontSize.Should().BeGreaterThan(0);
        pref.Primary.Should().BeNull();
    }
}
