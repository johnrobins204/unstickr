namespace Unstickd.Models;

public class Account
{
    public int Id { get; set; }
    public string Name { get; set; } = "Main User";
    
    // AI Configuration
    public string OllamaUrl { get; set; } = "http://localhost:11434";
    public string OllamaModel { get; set; } = "llama3";

    // UX Configuration
    public int? ActiveThemeId { get; set; }
    public Theme? ActiveTheme { get; set; }
    
    public ICollection<Story> Stories { get; set; } = new List<Story>();
    public ICollection<Notebook> Notebooks { get; set; } = new List<Notebook>();
}
