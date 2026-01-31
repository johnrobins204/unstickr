using System.Text.RegularExpressions;

namespace StoryFort.Services;

/// <summary>
/// Lightweight helper that wraps sentences in spans with stable IDs for the reader UI.
/// This is intentionally simple — for production you may want a proper tokenizer.
/// </summary>
public class ReaderHtmlHelper
{
    private static readonly Regex sentenceSplit = new(@"(?<=[\.\?\!])\s+", RegexOptions.Compiled);

    // Wrap sentences and produce deterministic sentence IDs using SHA1 of the sentence text.
    // This implementation is HTML-aware: it preserves inline tags (e.g., <em>, <strong>) inside sentence spans.
    public string WrapSentences(string html, IEnumerable<string>? pinnedIds = null)
    {
        if (string.IsNullOrWhiteSpace(html)) return string.Empty;

        var pinned = pinnedIds != null ? new HashSet<string>(pinnedIds) : new HashSet<string>();

        // Tokenize into tags and text nodes
        var tokenRegex = new Regex("(<[^>]+>)|([^<]+)", RegexOptions.Compiled);
        var tokens = tokenRegex.Matches(html).Cast<Match>().Select(m => new { IsTag = m.Groups[1].Success, Text = m.Value }).ToList();

        var sb = new System.Text.StringBuilder();

        var currentTokens = new List<(bool IsTag, string Text)>();

        void EmitCurrentSentence()
        {
            if (!currentTokens.Any()) return;

            // Build plain text for ID computation
            var plain = string.Concat(currentTokens.Where(t => !t.IsTag).Select(t => t.Text));
            var trimmedPlain = plain.Trim();
            if (string.IsNullOrEmpty(trimmedPlain))
            {
                currentTokens.Clear();
                return;
            }

            var sid = ComputeDeterministicId(trimmedPlain);
            var pinnedClass = pinned.Contains(sid) ? " pinned" : string.Empty;

            // Reconstruct HTML for this sentence: include tags as-is and HTML-encode text nodes
            var inner = new System.Text.StringBuilder();
            foreach (var t in currentTokens)
            {
                if (t.IsTag) inner.Append(t.Text);
                else inner.Append(System.Net.WebUtility.HtmlEncode(t.Text));
            }

            sb.Append($"<span class=\"reader-sentence{pinnedClass}\" data-sid=\"{sid}\" onclick=\"window.reader.onSentenceClicked('{sid}')\">{inner}</span>");
            // Preserve a single space after sentence for readability
            sb.Append(" ");
            currentTokens.Clear();
        }

        foreach (var tok in tokens)
        {
            if (tok.IsTag)
            {
                // Tags are part of current sentence
                currentTokens.Add((true, tok.Text));
                continue;
            }

            // Text node: may contain multiple sentence fragments
            var text = tok.Text;
            var parts = sentenceSplit.Split(text);
            if (parts.Length == 1)
            {
                currentTokens.Add((false, parts[0]));
            }
            else
            {
                for (int i = 0; i < parts.Length; i++)
                {
                    var part = parts[i];
                    currentTokens.Add((false, part));
                    // if not last part, this fragment ended in a sentence boundary
                    if (i < parts.Length - 1)
                    {
                        EmitCurrentSentence();
                    }
                }
            }
        }

        // Emit any trailing content as a final sentence
        EmitCurrentSentence();

        return sb.ToString();
    }

    private static string ComputeDeterministicId(string text)
    {
        using var sha = System.Security.Cryptography.SHA1.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(text.Trim());
        var hash = sha.ComputeHash(bytes);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
