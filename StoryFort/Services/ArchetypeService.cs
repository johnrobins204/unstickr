using StoryFort.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using StoryFort.Data;

namespace StoryFort.Services;

// Defines the shapes of stories (Hero's Journey, etc.)
public class ArchetypeService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ArchetypeService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public List<ArchetypeDefinition> GetArchetypes()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var archetypes = db.Archetypes
            .Include(a => a.Points)
            .ThenInclude(p => p.Examples)
            .ToList();

        // Convert DB Models to DTOs for UI
        return archetypes.Select(a => new ArchetypeDefinition 
        {
            Id = a.Id,
            Name = a.Name,
            Description = a.Description,
            Path = a.SvgPath,
            Points = a.Points.Select(p => new StoryPlotPoint 
            {
                Id = p.StepId, // Use StepId for the UI logic
                Label = p.Label,
                Prompt = p.Prompt,
                X = p.X,
                Y = p.Y,
                Align = p.Align,
                Examples = p.Examples?.ToList() ?? new List<ArchetypeExample>()
            }).OrderBy(p => p.Id).ToList()
        }).ToList();
    }
}

public class ArchetypeDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public List<StoryPlotPoint> Points { get; set; } = new();
}

