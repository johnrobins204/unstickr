namespace Unstickd.Models;

public class NotebookEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = "New Entity";
    public string Description { get; set; } = string.Empty;

    public int NotebookId { get; set; }
    public Notebook? Notebook { get; set; }

    public ICollection<NotebookEntry> Entries { get; set; } = new List<NotebookEntry>();

    // Navigation property for continuity aide (FR-4.3)
    public ICollection<StoryEntityLink> StoryLinks { get; set; } = new List<StoryEntityLink>();
    // Flexible entity metadata (MVP)
    public string Metadata { get; set; } = "{}";
}
