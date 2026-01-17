namespace Unstickd.Models;

public class Notebook
{
    public int Id { get; set; }
    public string Name { get; set; } = "General Notes";
    public string Icon { get; set; } = "bi-journal"; // Bootstrap Icon class

    public int AccountId { get; set; }
    public Account? Account { get; set; }

    public ICollection<NotebookEntity> Entities { get; set; } = new List<NotebookEntity>();
}
