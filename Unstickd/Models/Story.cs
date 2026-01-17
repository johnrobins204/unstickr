namespace Unstickd.Models;

public class Story
{
    public int Id { get; set; }
    public string Title { get; set; } = "Untitled Story";
    public DateTime Created { get; set; } = DateTime.Now;
    public DateTime LastModified { get; set; } = DateTime.Now;

    public int ThemeId { get; set; }
    public Theme? Theme { get; set; }

    public int AccountId { get; set; }
    public Account? Account { get; set; }

    public ICollection<StoryPage> Pages { get; set; } = new List<StoryPage>();
    public ICollection<StoryEntityLink> EntityLinks { get; set; } = new List<StoryEntityLink>();
}
