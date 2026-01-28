namespace StoryFort.Models;

public class ThemePreference
{
    public string? Background { get; set; }
    public string? Foreground { get; set; }
    public string? Primary { get; set; }
    public string? Card { get; set; }
    public string? FontClass { get; set; }
    public int FontSize { get; set; } = 16;
    public string? Radius { get; set; }
}

