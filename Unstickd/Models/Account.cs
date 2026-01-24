namespace Unstickd.Models;

public class Account
{
    public int Id { get; set; }
    public string Name { get; set; } = "Main User";
    // Adult supervisor fields (MVP)
    public string SupervisorName { get; set; } = string.Empty;
    public string SupervisorEmail { get; set; } = string.Empty;
    
    // AI Configuration
    public string CohereApiKey { get; set; } = string.Empty;
    public bool UseReasoningModel { get; set; } = true;

    // UX Configuration
    public int? ActiveThemeId { get; set; }
    public Theme? ActiveTheme { get; set; }
    
    public ICollection<Story> Stories { get; set; } = new List<Story>();
    public ICollection<Notebook> Notebooks { get; set; } = new List<Notebook>();
}
