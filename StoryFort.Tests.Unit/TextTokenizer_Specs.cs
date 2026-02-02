using System;
using FluentAssertions;
using StoryFort.Services;
using Xunit;

namespace StoryFort.Tests.Unit
{
    /// <summary>
    /// Spec: /specs/texttokenizer.md
    /// </summary>
    public class TextTokenizer_Specs
    {
        [Fact]
        public void PatchHtml_ReplacesWordAndPreservesFormatting()
        {
            var tokenizer = new TextTokenizer();
            var html = "<p>Hello <strong>world</strong>!</p>";
            var patched = tokenizer.PatchHtml(html, 1, "UNIVERSE");
            patched.Should().Be("<p>Hello <strong>UNIVERSE</strong>!</p>");
        }

        [Fact]
        public void PatchHtml_PreservesPunctuation()
        {
            var tokenizer = new TextTokenizer();
            var html = "<p>Wow <em>space</em>!</p>";
            var patched = tokenizer.PatchHtml(html, 1, "WORLD");
            patched.Should().Be("<p>Wow <em>WORLD</em>!</p>");
        }

        [Fact]
        public void PatchHtml_EscapesUserSuppliedHtmlLikeInput()
        {
            var tokenizer = new TextTokenizer();
            var html = "<p>Hello <strong>world</strong>!</p>";
            var malicious = "UNIVERSE!</body>";
            var patched = tokenizer.PatchHtml(html, 1, malicious);

            // The replacement must be treated as literal text and any angle brackets escaped.
            patched.Should().Contain("UNIVERSE!");
            patched.Should().NotContain("</body>");
            patched.Should().Contain("&lt;/body&gt;");

            // Still preserve outer structure
            patched.Should().StartWith("<p>");
            patched.Should().EndWith("</p>");
        }
    }
}
