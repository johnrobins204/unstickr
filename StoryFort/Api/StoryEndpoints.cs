using Microsoft.EntityFrameworkCore;
using StoryFort.Data;
using StoryFort.Models;
using StoryFort.Services;

namespace StoryFort.Api;

public static class StoryEndpoints
{
    public static void MapStoryEndpoints(this WebApplication app)
    {
        app.MapGet("/api/notebooks", async (int? accountId, AppDbContext db) =>
        {
            var acct = accountId ?? 1;
            var list = await db.Notebooks
                .Where(n => n.AccountId == acct)
                .Select(n => new { id = n.Id, name = n.Name })
                .ToListAsync();
            return Results.Json(list);
        });

        app.MapPost("/api/pins", async (HttpRequest request, AppDbContext db, ToastService toastService, AchievementService achievementService) =>
        {
            var accountId = 1; // single-user mode
            using var doc = await System.Text.Json.JsonDocument.ParseAsync(request.Body);
            var root = doc.RootElement;
            int? notebookId = null;
            if (root.TryGetProperty("notebookId", out var nbp) && nbp.ValueKind == System.Text.Json.JsonValueKind.Number)
                notebookId = nbp.GetInt32();
            int storyId = 0;
            if (root.TryGetProperty("storyId", out var sp) && sp.ValueKind == System.Text.Json.JsonValueKind.Number)
                storyId = sp.GetInt32();
            string content = root.TryGetProperty("content", out var cp) ? cp.GetString() ?? string.Empty : string.Empty;
            string? sentenceId = root.TryGetProperty("sentenceId", out var sp2) ? sp2.GetString() : null;

            Notebook? notebook = null;
            if (notebookId.HasValue)
            {
                notebook = await db.Notebooks.FindAsync(notebookId.Value);
            }
            if (notebook == null)
            {
                notebook = await db.Notebooks.FirstOrDefaultAsync(n => n.AccountId == accountId && n.Name == "Quick Pins");
                if (notebook == null)
                {
                    notebook = new Notebook { Name = "Quick Pins", Icon = "bi-pin", AccountId = accountId };
                    db.Notebooks.Add(notebook);
                    await db.SaveChangesAsync();
                }
            }

            var entity = await db.NotebookEntities.FirstOrDefaultAsync(e => e.NotebookId == notebook.Id && e.Name == "Pins");
            if (entity == null)
            {
                entity = new NotebookEntity { Name = "Pins", NotebookId = notebook.Id };
                db.NotebookEntities.Add(entity);
                await db.SaveChangesAsync();
            }

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

            try
            {
                toastService.Show("Saved to Notebook", "Saved to Quick Pins", "primary", 3000);
            }
            catch { }

            try
            {
                if (!string.IsNullOrWhiteSpace(sentenceId))
                {
                    var unlocked = await achievementService.UnlockBadgeAsync(accountId, "research_assistant", "Research Assistant");
                    if (unlocked) toastService.Show("Badge Unlocked", "Research Assistant — first pin", "danger", 4500);
                }
            }
            catch { }

            return Results.Created($"/api/pins/{entry.Id}", new { id = entry.Id });
        });
    }
}
