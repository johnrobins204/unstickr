using System.Collections.Generic;
using StoryFort.Models;

namespace StoryFort.Data;

public class SeedDto
{
    public List<Archetype>? Archetypes { get; set; }
    public List<ArchetypePoint>? ArchetypePoints { get; set; }
    public List<ArchetypeExample>? ArchetypeExamples { get; set; }
    public List<NotebookType>? NotebookTypes { get; set; }
    public List<Theme>? Themes { get; set; }
    public List<Account>? Accounts { get; set; }
}