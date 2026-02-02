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
        try
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
                Id = a.Id ?? string.Empty,
                Name = a.Name ?? string.Empty,
                Description = a.Description ?? string.Empty,
                Path = a.SvgPath ?? string.Empty,
                PlaceOfOrigin = a.PlaceOfOrigin ?? string.Empty,
                Points = (a.Points ?? new List<ArchetypePoint>())
                    .Select(p => new StoryPlotPoint 
                    {
                        Id = p.StepId,
                        Label = p.Label ?? string.Empty,
                        Prompt = p.Prompt ?? string.Empty,
                        X = p.X,
                        Y = p.Y,
                        Align = p.Align ?? string.Empty,
                        Examples = p.Examples?.ToList() ?? new List<ArchetypeExample>()
                    }).OrderBy(p => p.Id).ToList()
            }).ToList();
        }
        catch (Exception ex)
        {
            // On any error reading archetypes (malformed seed, DB issues), fail safe to empty list
            // Log the exception for observability (in production, use ILogger)
            System.Diagnostics.Debug.WriteLine($"ArchetypeService.GetArchetypes failed: {ex.Message}");
            return new List<ArchetypeDefinition>();
        }
    }

    public ArchetypeDefinition? GetArchetypeById(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        var list = GetArchetypes();
        return list.FirstOrDefault(a => string.Equals(a.Id, id, StringComparison.OrdinalIgnoreCase));
    }
}

public class ArchetypeDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string PlaceOfOrigin { get; set; } = string.Empty;
    public List<StoryPlotPoint> Points { get; set; } = new();
}

