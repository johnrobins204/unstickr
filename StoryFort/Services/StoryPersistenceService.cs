using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using StoryFort.Data;
using StoryFort.Models;

namespace StoryFort.Services;

public class StoryPersistenceService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public StoryPersistenceService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<Story?> LoadStoryAsync(int storyId)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await db.Stories
            .Include(s => s.Account)
            .ThenInclude(acc => acc!.ActiveTheme)
            .Include(s => s.EntityLinks)
            .Include(s => s.Account)
            .ThenInclude(a => a!.Notebooks)
            .ThenInclude(n => n!.Entities)
            .ThenInclude(e => e!.Entries)
            .FirstOrDefaultAsync(s => s.Id == storyId);
    }

    public async Task SaveContentAsync(int storyId, string html, string? title, string? genre)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var story = await db.Stories.FindAsync(storyId);
        if (story == null) return;

        story.Content = html;
        if (!string.IsNullOrWhiteSpace(title)) story.Title = title!;
        if (!string.IsNullOrWhiteSpace(genre)) story.Genre = genre!;
        story.LastModified = DateTime.Now;

        await db.SaveChangesAsync();
    }

    // Centeralized update to ensure EF Value Converters are triggered correctly if needed, 
    // and to keep logic out of Razor components.
    public async Task UpdateEntityAsync(NotebookEntity entity)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.NotebookEntities.Update(entity);
        await db.SaveChangesAsync();
    }

    public async Task<string?> LoadMetadataAsync(int storyId)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var story = await db.Stories.FindAsync(storyId);
        return story?.Metadata;
    }

    public async Task SaveMetadataAsync(int storyId, string metadata)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var story = await db.Stories.FindAsync(storyId);
        if (story == null) return;
        story.Metadata = metadata;
        story.LastModified = DateTime.Now;
        await db.SaveChangesAsync();
    }

    public async Task<List<Notebook>> GetNotebooksAsync(int accountId)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.Notebooks.Where(n => n.AccountId == accountId).ToListAsync();
    }

    public async Task<Notebook> EnsureNotebookAsync(int accountId, string name = "Quick Pins", string icon = "bi-pin")
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var notebook = await db.Notebooks.FirstOrDefaultAsync(n => n.AccountId == accountId && n.Name == name);
        if (notebook == null)
        {
            notebook = new Notebook { Name = name, Icon = icon, AccountId = accountId, LastModified = DateTime.Now };
            db.Notebooks.Add(notebook);
            await db.SaveChangesAsync();
        }

        return notebook;
    }

    public async Task<NotebookEntity> EnsureEntityAsync(int notebookId, string name)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var entity = await db.NotebookEntities.FirstOrDefaultAsync(e => e.NotebookId == notebookId && e.Name == name);
        if (entity == null)
        {
            entity = new NotebookEntity { Name = name, NotebookId = notebookId };
            db.NotebookEntities.Add(entity);
            await db.SaveChangesAsync();
        }

        return entity;
    }

    public async Task<NotebookEntry> AddNotebookEntryAsync(int? notebookId, int? storyId, string content, string entityName = "Pins", int accountId = 1, string? sentenceId = null)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Notebook? notebook = null;
        if (notebookId.HasValue)
        {
            notebook = await db.Notebooks.FindAsync(notebookId.Value);
        }

        if (notebook == null)
        {
            notebook = await EnsureNotebookAsync(accountId, "Quick Pins", "bi-pin");
        }

        var entity = await EnsureEntityAsync(notebook.Id, entityName);

        var entry = new NotebookEntry
        {
            Content = content,
            Created = DateTime.Now,
            NotebookEntityId = entity.Id,
            StoryId = storyId > 0 ? storyId : null
        };

        db.NotebookEntries.Add(entry);
        await db.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(sentenceId) && entry.StoryId.HasValue)
        {
            var story = await db.Stories.FindAsync(entry.StoryId.Value);
            if (story != null)
            {
                try
                {
                    var metadata = new System.Text.Json.Nodes.JsonObject();
                    if (!string.IsNullOrWhiteSpace(story.Metadata))
                    {
                        var existing = System.Text.Json.JsonDocument.Parse(story.Metadata);
                        foreach (var prop in existing.RootElement.EnumerateObject())
                        {
                            metadata[prop.Name] = System.Text.Json.Nodes.JsonNode.Parse(prop.Value.GetRawText());
                        }
                    }

                    var pinned = metadata["pinnedSentences"] as System.Text.Json.Nodes.JsonObject;
                    if (pinned == null)
                    {
                        pinned = new System.Text.Json.Nodes.JsonObject();
                        metadata["pinnedSentences"] = pinned;
                    }
                    pinned[sentenceId] = entry.Id;

                    story.Metadata = metadata.ToJsonString();
                    db.Stories.Update(story);
                    await db.SaveChangesAsync();
                }
                catch
                {
                    // ignore metadata failures
                }
            }
        }

        return entry;
    }
}
