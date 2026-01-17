namespace Unstickd.Models;

public class StoryEntityLink
{
    public int StoryId { get; set; }
    public Story? Story { get; set; }

    public int NotebookEntityId { get; set; }
    public NotebookEntity? NotebookEntity { get; set; }
}
