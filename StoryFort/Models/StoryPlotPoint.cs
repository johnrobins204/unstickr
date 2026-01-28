using System.Collections.Generic;

namespace StoryFort.Models;

// Represents a single point on the plot mountain
public class StoryPlotPoint
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public string? Align { get; set; }
    public List<ArchetypeExample> Examples { get; set; } = new();
}

public class StoryPlanData
{
    public string ArchetypeId { get; set; } = "hero";
    public string ArchetypeDescription { get; set; } = string.Empty;
    public List<StoryPlotPoint> PlotPoints { get; set; } = new();
}

