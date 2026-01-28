namespace StoryFort.Models;

public class NotebookEntry
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty; // HTML or Text
    public DateTime Created { get; set; } = DateTime.Now;
    
    public int NotebookEntityId { get; set; }
    public NotebookEntity? NotebookEntity { get; set; }

    public int? StoryId { get; set; }
    public Story? Story { get; set; }
}

