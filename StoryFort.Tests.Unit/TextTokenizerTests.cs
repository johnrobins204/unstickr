using System;
using FluentAssertions;
using StoryFort.Services;
using Xunit;

namespace StoryFort.Tests.Unit;

public class TextTokenizerTests
{
    [Fact]
    public void TokenizeHtml_SplitsWordsAndPreservesTags()
    {
        var html = "<p>Hello <strong>world</strong>!</p>";
        var tokenizer = new TextTokenizer();
        var tokens = tokenizer.TokenizeHtml(html);

        tokens.Should().HaveCount(3);
        tokens[0].Text.Should().Be("Hello");
        tokens[1].Text.Should().Be("world");
        tokens[2].Text.Should().Be("!");
        tokens[1].HtmlTag.Should().Be("strong");
        tokens[2].IsPunctuation.Should().BeTrue();
    }

    [Fact]
    public void PatchHtml_ReplacesCorrectToken()
    {
        var html = "<p>One two three</p>";
        var tokenizer = new TextTokenizer();
        var patched = tokenizer.PatchHtml(html, 1, "TWO");

        patched.Should().Contain("One TWO three");
    }

    [Fact]
    public void TokenizeHtml_HandlesEmptyString()
    {
        var tokenizer = new TextTokenizer();
        var tokens = tokenizer.TokenizeHtml("");
        tokens.Should().BeEmpty();
    }
}

