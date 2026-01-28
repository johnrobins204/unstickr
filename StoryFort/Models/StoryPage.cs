namespace StoryFort.Models;

public class StoryPage
{
    public int Id { get; set; }
    public int PageNumber { get; set; }
    public string Content { get; set; } = string.Empty; // HTML
    public DateTime LastModified { get; set; } = DateTime.Now;
    
    public int StoryId { get; set; }
    public Story? Story { get; set; }
}

