namespace Unstickd.Models;

public class Account
{
    public int Id { get; set; }
    public string Name { get; set; } = "Main User";
    
    public ICollection<Story> Stories { get; set; } = new List<Story>();
    public ICollection<Notebook> Notebooks { get; set; } = new List<Notebook>();
}
