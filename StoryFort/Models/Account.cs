using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace StoryFort.Models;

public class Account
{
    public int Id { get; set; }
    public string Name { get; set; } = "Main User";
    // Adult supervisor fields (MVP)
    public string SupervisorName { get; set; } = string.Empty;
    public string SupervisorEmail { get; set; } = string.Empty;
    
    // AI Configuration
    // Persist the protected key; runtime code should use IApiKeyProtector.Unprotect when needed.
    public string ProtectedCohereApiKey { get; set; } = string.Empty;

    public bool UseReasoningModel { get; set; } = true;

    // UX Configuration
    public int? ActiveThemeId { get; set; }
    public Theme? ActiveTheme { get; set; }
    public string ThemePreferenceJson { get; set; } = "{}";
    [NotMapped]
    public ThemePreference ThemePreference
    {
        get
        {
            try
            {
                return JsonSerializer.Deserialize<ThemePreference>(ThemePreferenceJson) ?? new ThemePreference();
            }
            catch
            {
                return new ThemePreference();
            }
        }
        set
        {
            ThemePreferenceJson = JsonSerializer.Serialize(value);
        }
    }
    
    public ICollection<Story> Stories { get; set; } = new List<Story>();
    public ICollection<Notebook> Notebooks { get; set; } = new List<Notebook>();
}

