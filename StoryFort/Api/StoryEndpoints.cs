using Microsoft.EntityFrameworkCore;
using StoryFort.Data;
using StoryFort.Models;
using StoryFort.Services;

namespace StoryFort.Api;

public static class StoryEndpoints
{
    public record UpdatePinsRequest(int? notebookId, int? storyId, string? content, string? sentenceId);
    public record CohereKeyRequest(string? key, bool archiveOld = false);

    public static void MapStoryEndpoints(this WebApplication app)
    {
        app.MapGet("/api/notebooks", async (int? accountId, StoryPersistenceService persistence) =>
        {
            var acct = accountId ?? 1;
            var notebooks = await persistence.GetNotebooksAsync(acct);
            var list = notebooks.Select(n => new { id = n.Id, name = n.Name });
            return Results.Json(list);
        });

        app.MapPost("/api/pins", async (UpdatePinsRequest body, StoryPersistenceService persistence, ToastService toastService, AchievementService achievementService) =>
        {
            var accountId = 1; // single-user mode
            var entry = await persistence.AddNotebookEntryAsync(body.notebookId, body.storyId, body.content ?? string.Empty, "Pins", accountId, body.sentenceId);

            try
            {
                toastService.Show("Saved to Notebook", "Saved to Quick Pins", "primary", 3000);
            }
            catch { }

            try
            {
                if (!string.IsNullOrWhiteSpace(body.sentenceId))
                {
                    var unlocked = await achievementService.UnlockBadgeAsync(accountId, "research_assistant", "Research Assistant");
                    if (unlocked) toastService.Show("Badge Unlocked", "Research Assistant — first pin", "danger", 4500);
                }
            }
            catch { }

            return Results.Created($"/api/pins/{entry.Id}", new { id = entry.Id });
        });

        app.MapPost("/api/accounts/{id}/cohere-key", async (int id, CohereKeyRequest body, AppDbContext db, IApiKeyProtector protector) =>
        {
            var key = body.key ?? string.Empty;
            var archive = body.archiveOld;

            if (string.IsNullOrWhiteSpace(key)) return Results.BadRequest(new { error = "key is required" });

            var account = await db.Accounts.FindAsync(id);
            if (account == null) return Results.NotFound();

            // Archive previous protected key if present
            if (!string.IsNullOrWhiteSpace(account.ProtectedCohereApiKey) && archive)
            {
                var hist = new StoryFort.Models.AccountApiKeyHistory
                {
                    AccountId = account.Id,
                    ProtectedKey = account.ProtectedCohereApiKey,
                    CreatedUtc = DateTime.UtcNow,
                    IsActive = false
                };
                db.AccountApiKeyHistories.Add(hist);
            }

            // Protect and store new key
            account.ProtectedCohereApiKey = protector.Protect(key);
            // Do not set account.CohereApiKey (runtime-only)
            db.Accounts.Update(account);
            await db.SaveChangesAsync();

            return Results.Ok(new { status = "ok" });
        });
    }
}
