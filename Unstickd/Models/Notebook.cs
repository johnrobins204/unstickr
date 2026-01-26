namespace Unstickd.Models;

public class Notebook
{
    public int Id { get; set; }
    public string Name { get; set; } = "General Notes";
    public string Icon { get; set; } = "bi-journal"; // Bootstrap Icon class
    
    public bool IsSystem { get; set; } = false;
    public DateTime LastModified { get; set; } = DateTime.Now;

    public int AccountId { get; set; }
    public Account? Account { get; set; }

    public ICollection<NotebookEntity> Entities { get; set; } = new List<NotebookEntity>();
}
