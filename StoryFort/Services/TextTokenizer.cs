using HtmlAgilityPack;
using System.Text;

namespace StoryFort.Services;

public class TextTokenizer
{
    // Patches the HTML by replacing the word at the given token index with newText
    public string PatchHtml(string html, int tokenIndex, string newText)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        int index = 0;
        PatchTraverse(doc.DocumentNode, ref index, tokenIndex, newText);
        return doc.DocumentNode.OuterHtml;
    }

    private bool PatchTraverse(HtmlNode node, ref int index, int targetIndex, string newText)
    {
        if (node.NodeType == HtmlNodeType.Text)
        {
            var words = node.InnerText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                if (index == targetIndex)
                {
                    // HTML-encode user-supplied replacement to avoid injecting raw HTML
                    var encoded = System.Net.WebUtility.HtmlEncode(newText);
                    words[i] = encoded;
                    // Set the text content safely by assigning encoded HTML
                    node.InnerHtml = string.Join(" ", words);
                    return true; // Patched
                }
                index++;
            }
        }
        else
        {
            foreach (var child in node.ChildNodes)
            {
                if (PatchTraverse(child, ref index, targetIndex, newText))
                    return true;
            }
        }
        return false;
    }

    public class Token
    {
        public string Text { get; set; } = string.Empty;
        public int Index { get; set; }
        public string? HtmlTag { get; set; }
        public bool IsPunctuation { get; set; }
    }

    // Tokenizes HTML into a list of tokens, preserving structure
    public List<Token> TokenizeHtml(string html)
    {
        var tokens = new List<Token>();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        int index = 0;
        Traverse(doc.DocumentNode, ref tokens, ref index);
        return tokens;
    }

    private void Traverse(HtmlNode node, ref List<Token> tokens, ref int index)
    {
        if (node.NodeType == HtmlNodeType.Text)
        {
            var words = node.InnerText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in words)
            {
                tokens.Add(new Token
                {
                    Text = word,
                    Index = index++,
                    HtmlTag = node.ParentNode.Name,
                    IsPunctuation = word.Length == 1 && char.IsPunctuation(word[0])
                });
            }
        }
        else
        {
            foreach (var child in node.ChildNodes)
            {
                Traverse(child, ref tokens, ref index);
            }
        }
    }
}
