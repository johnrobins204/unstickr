using FluentAssertions;
using StoryFort.Services;
using Xunit;

namespace StoryFort.Tests.Unit;

public class ReaderHtmlHelperTests
{
    [Fact]
    public void WrapSentences_EmptyInput_ReturnsEmpty()
    {
        var helper = new ReaderHtmlHelper();
        helper.WrapSentences(string.Empty).Should().BeEmpty();
    }

    [Fact]
    public void WrapSentences_SplitsAndWrapsSentences()
    {
        var helper = new ReaderHtmlHelper();
        var input = "<p>Hello world. This is a test! <em>Is it?</em></p>";
        var output = helper.WrapSentences(input);
        output.Should().Contain("reader-sentence");
        // Should produce at least 3 spans
        output.Split("reader-sentence").Length.Should().BeGreaterThan(3);
    }

    [Fact]
    public void WrapSentences_PinnedId_AddsPinnedClass()
    {
        var helper = new ReaderHtmlHelper();
        var input = "<p>One. Two.</p>";
        var raw = helper.WrapSentences(input);
        // extract a sid from the first occurrence
        var idx = raw.IndexOf("data-sid=\"");
        raw.Should().Contain("data-sid=");
        var sidStart = raw.Substring(idx + 10, 40);
        var sid = sidStart.Split('"')[0];

        var pinned = helper.WrapSentences(input, new[] { sid });
        pinned.Should().Contain("pinned");
    }
}

