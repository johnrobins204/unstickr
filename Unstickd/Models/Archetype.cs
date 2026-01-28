using System.ComponentModel.DataAnnotations;

namespace Unstickd.Models;

public class Archetype
{
    [Key]
    public string Id { get; set; } = string.Empty; // e.g., "hero", "classic"
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SvgPath { get; set; } = string.Empty;
    
    public ICollection<ArchetypePoint> Points { get; set; } = new List<ArchetypePoint>();
}

public class ArchetypePoint
{
    public int Id { get; set; } // DB Key
    
    public string ArchetypeId { get; set; } = string.Empty;
    public Archetype? Archetype { get; set; }

    public int StepId { get; set; } // The logical link to user's StoryPlan (e.g., 1, 2, 3)
    public string Label { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    
    // Coordinates for the Map
    public double X { get; set; }
    public double Y { get; set; }
    public string Align { get; set; } = "center";
    
    public ICollection<ArchetypeExample> Examples { get; set; } = new List<ArchetypeExample>();
}
