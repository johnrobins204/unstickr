using Unstickd.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Unstickd.Data;

namespace Unstickd.Services;

public class TutorSession
{
    public TutorMode CurrentMode { get; set; } = TutorMode.Idle;
    public List<string> History { get; set; } = new();
    public string? LastJsonStatus { get; set; }
}
public class StoryState
{
    private readonly IServiceScopeFactory _scopeFactory;

    public StoryState(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    // Writing flow state for sidebar (reading/checking/helping)
    public string FlowState { get; set; } = "reading";
    // Basic story data held in memory per user session
    public int StoryId { get; set; }
    public string Title { get; set; } = "Untitled Story";
    public Account? Account { get; set; }

    // HTML content from the Rich Text Editor
    public string Content { get; set; } = "<p>Once upon a time...</p>";

    // Simple scratchpad for the session
    public string TutorNotes { get; set; } = string.Empty;

    // Event to notify components of changes (useful if multiple components display this data)
    public event Action? OnChange;
    public void NotifyStateChanged() => OnChange?.Invoke();

    // Notebooks (In-Memory for faster access in session)
    public List<Notebook> Notebooks { get; set; } = new List<Notebook>();

    // IDs of entities relevant to this specific story
    public HashSet<int> LinkedEntityIds { get; set; } = new HashSet<int>();

    // Current Theme for Dynamic Styling
    public Theme? CurrentTheme { get; set; }

    // Design Lab: Theme Preference (persisted)
    public ThemePreference ThemePreference { get; set; } = new();

    // Debounce timer for theme changes
    private System.Timers.Timer? _themeDebounceTimer;
    private const int ThemeDebounceMs = 2000;
    public void UpdateThemePreference(Action<ThemePreference> update)
    {
        update(ThemePreference);
        NotifyStateChanged();
        DebounceThemeSave();
    }
    private void DebounceThemeSave()
    {
        _themeDebounceTimer?.Stop();
        _themeDebounceTimer = _themeDebounceTimer ?? new System.Timers.Timer(ThemeDebounceMs);
        _themeDebounceTimer.AutoReset = false;
        _themeDebounceTimer.Elapsed += async (s, e) => await PersistThemePreference();
        _themeDebounceTimer.Start();
    }

    private async Task PersistThemePreference()
    {
        if (Account == null) return;
        
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            var account = await db.Accounts.FindAsync(Account.Id);
            if (account != null)
            {
                account.ThemePreferenceJson = JsonSerializer.Serialize(ThemePreference);
                await db.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving theme preference: {ex.Message}");
        }
    }

    // AI Tutor Mode
    // ...moved to Unstickd.Models.TutorEnums.cs...

    public TutorSession TutorSession { get; set; } = new();
    public string Genre { get; set; } = "General";

    // Added for prompt context
    public string? Age { get; set; } = null;
    public string? Archetype { get; set; } = null;
}
