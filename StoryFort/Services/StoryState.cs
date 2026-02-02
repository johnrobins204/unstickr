using StoryFort.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using StoryFort.Data;

namespace StoryFort.Services;

public class TutorSession
{
    public TutorMode CurrentMode { get; set; } = TutorMode.Idle;
    public List<string> History { get; set; } = new();
    public string? LastJsonStatus { get; set; }
}

// SessionState: UI-scoped state and lightweight persistence helpers
public class SessionState
{
    private readonly IServiceScopeFactory? _scopeFactory;

    public SessionState() { }
    public SessionState(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    // Writing flow state for sidebar (reading/checking/helping)
    public string FlowState { get; set; } = "reading";

    // Simple scratchpad for the session
    public string TutorNotes { get; set; } = string.Empty;

    // Event to notify components of changes
    public event Action? OnChange;
    public void NotifyStateChanged() => OnChange?.Invoke();

    // Notebooks (In-Memory for faster access in session)
    public List<Notebook> Notebooks { get; set; } = new List<Notebook>();

    // Current Theme for Dynamic Styling
    public Theme? CurrentTheme { get; set; }

    // Design Lab: Theme Preference (persisted)
    public ThemePreference ThemePreference { get; set; } = new();

    // Update theme preference and persist via ThemeService (no debounce for prototype)
    public void UpdateThemePreference(Action<ThemePreference> update, Account? account = null)
    {
        update(ThemePreference);
        NotifyStateChanged();

        if (account == null || _scopeFactory == null) return;

        // Fire-and-forget persistence to avoid blocking UI
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var themeService = scope.ServiceProvider.GetRequiredService<ThemeService>();
                await themeService.SaveThemePreferenceAsync(account.Id, ThemePreference);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error saving theme preference for Account {AccountId}", account?.Id);
            }
        });
    }
}

// StoryContext: domain-related state for the active story
public class StoryContext
{
    // Basic story data held in memory per user session
    public int StoryId { get; set; }
    public string Title { get; set; } = "Untitled Story";
    public Account? Account { get; set; }

    // HTML content from the Rich Text Editor
    public string Content { get; set; } = "<p>Once upon a time...</p>";

    // IDs of entities relevant to this specific story
    public HashSet<int> LinkedEntityIds { get; set; } = new HashSet<int>();

    public string Genre { get; set; } = "General";
    // Added for prompt context
    public string? Age { get; set; } = null;
    public string? Archetype { get; set; } = null;

    // Planner Data
    public StoryPlanData CurrentPlan { get; set; } = new();

    // Review Session: Holds tokenized words and flagged issues for the review phase
    public List<ReviewToken>? ReviewTokens { get; set; }
    public List<ReviewFlag>? ReviewFlags { get; set; }
    public void ClearReviewSession()
    {
        ReviewTokens = null;
        ReviewFlags = null;
    }
}

// Represents a tokenized word in the review phase
public class ReviewToken
{
    public int Index { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? HtmlTag { get; set; }
    public bool IsPunctuation { get; set; }
    public bool IsFlagged { get; set; } = false;
    public string? FlagType { get; set; } // e.g., Spelling, Grammar, Repetition
}

// Represents a flagged issue in the review phase
public class ReviewFlag
{
    public int TokenIndex { get; set; }
    public string Type { get; set; } = string.Empty; // Spelling, Grammar, Repetition
    public string? Note { get; set; }
    public bool IsResolved { get; set; } = false;
}

