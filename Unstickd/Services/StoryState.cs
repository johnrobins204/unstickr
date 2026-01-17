using Unstickd.Models;

namespace Unstickd.Services;

public class StoryState
{
    // Basic story data held in memory per user session
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
}
