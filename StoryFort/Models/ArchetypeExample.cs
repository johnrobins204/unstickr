using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StoryFort.Models;

public class ArchetypeExample
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int ArchetypePointId { get; set; }
    
    [ForeignKey("ArchetypePointId")]
    public ArchetypePoint? ArchetypePoint { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty; // e.g., "The Wizard of Oz"

    [Required]
    public string Content { get; set; } = string.Empty; // e.g., "Dorothy lives in Kansas..."
}

