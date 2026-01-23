using Unstickd.Models;

namespace Unstickd.Services;

public class TutorSession
{
    public TutorMode CurrentMode { get; set; } = TutorMode.Idle;
    public List<string> History { get; set; } = new();
    public string? LastJsonStatus { get; set; }
}

public class StoryState
{
    // Basic story data held in memory per user session
    public int StoryId { get; set; }
    public string Title { get; set; } = "Untitled Story";
    
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

    // AI Tutor Mode
    // ...moved to Unstickd.Models.TutorEnums.cs...

    public TutorSession TutorSession { get; set; } = new();
    public string Genre { get; set; } = "General";
}
