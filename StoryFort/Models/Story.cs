namespace StoryFort.Models;

public class Story
{
    public int Id { get; set; }
    public string Title { get; set; } = "Untitled Story";
    public DateTime Created { get; set; } = DateTime.Now;
    public DateTime LastModified { get; set; } = DateTime.Now;

    public int AccountId { get; set; }
    public Account? Account { get; set; }

    // New Continuous Content Field
    public string Content { get; set; } = "";
    // Pages removed: Content is now the single source of truth
    public ICollection<StoryEntityLink> EntityLinks { get; set; } = new List<StoryEntityLink>();

    public string Genre { get; set; } = "General";
    // Flexible story metadata (MVP)
    public string Metadata { get; set; } = "{}";
}

