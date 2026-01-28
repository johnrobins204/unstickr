namespace StoryFort.Models;

public class NotebookType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = "bi-journal";
    public bool IsSystemDefault { get; set; } = false;
}

