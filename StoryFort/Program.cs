using StoryFort.Components;
using StoryFort.Services;
using StoryFort.Data;
using Blazored.TextEditor;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Server.Circuits;

// 1. Setup Serilog ASAP
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/StoryFort-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// 2. Clear default providers and use Serilog
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Enhanced Debugging
builder.Services.AddScoped<IErrorBoundaryLogger, CustomErrorBoundaryLogger>();
builder.Services.AddScoped<CircuitHandler, UserCircuitHandler>();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=StoryFort.db"));

// State Management
builder.Services.AddScoped<StoryState>();
builder.Services.AddSingleton<ArchetypeService>();
builder.Services.AddSingleton<TextTokenizer>();
builder.Services.AddScoped<TutorOrchestrator>();
builder.Services.AddScoped<ICohereTutorService, CohereTutorService>();
// Reader helper service
builder.Services.AddSingleton<ReaderHtmlHelper>();
// Library Seeder
builder.Services.AddSingleton<LibrarySeeder>();
// Toasts & Achievements
builder.Services.AddSingleton<ToastService>();
builder.Services.AddScoped<AchievementService>();

// LLM Integration Service
builder.Services.AddHttpClient("LLM", client => 
{
    client.BaseAddress = new Uri("https://api.cohere.com/");
    client.Timeout = TimeSpan.FromMinutes(5); // LLMs can be slow
});

// Database (Future To-Do: AddDbContext<AppDbContext>(...))
// builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=StoryFort.db"));

var app = builder.Build();

// Minimal API for reader pinning + notebook lookup
app.MapGet("/api/notebooks", async (int? accountId, AppDbContext db) =>
{
    var acct = accountId ?? 1;
    var list = await db.Notebooks
        .Where(n => n.AccountId == acct)
        .Select(n => new { id = n.Id, name = n.Name })
        .ToListAsync();
    return Results.Json(list);
});

app.MapPost("/api/pins", async (HttpRequest request, AppDbContext db, StoryFort.Services.ToastService toastService, StoryFort.Services.AchievementService achievementService) =>
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

    // Resolve or create notebook
    StoryFort.Models.Notebook? notebook = null;
    if (notebookId.HasValue)
    {
        notebook = await db.Notebooks.FindAsync(notebookId.Value);
    }
    if (notebook == null)
    {
        notebook = await db.Notebooks.FirstOrDefaultAsync(n => n.AccountId == accountId && n.Name == "Quick Pins");
        if (notebook == null)
        {
            notebook = new StoryFort.Models.Notebook { Name = "Quick Pins", Icon = "bi-pin", AccountId = accountId };
            db.Notebooks.Add(notebook);
            await db.SaveChangesAsync();
        }
    }

    // Ensure a "Pins" entity exists under the notebook
    var entity = await db.NotebookEntities.FirstOrDefaultAsync(e => e.NotebookId == notebook.Id && e.Name == "Pins");
    if (entity == null)
    {
        entity = new StoryFort.Models.NotebookEntity { Name = "Pins", NotebookId = notebook.Id };
        db.NotebookEntities.Add(entity);
        await db.SaveChangesAsync();
    }

    var entry = new StoryFort.Models.NotebookEntry
    {
        Content = content,
        Created = DateTime.Now,
        NotebookEntityId = entity.Id,
        StoryId = storyId > 0 ? storyId : null
    };

    db.NotebookEntries.Add(entry);
    await db.SaveChangesAsync();

    // Persist a mapping of pinned sentence -> entry id in the Story.Metadata JSON to enable client-side indicators.
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

    // Notify UI via toast
    try
    {
        toastService.Show("Saved to Notebook", "Saved to Quick Pins", "primary", 3000);
    }
    catch { }

    // Unlock research badge on first pin
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

// === PILOT HARDENING: Enable WAL Mode for Concurrency ===
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // Ensure created and migrate
    db.Database.Migrate();

    // Enable WAL Mode (Write-Ahead Logging) to allow concurrent readers/writers
    // This prevents SQLITE_BUSY errors when 30+ students save simultaneously.
    db.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");

    // Seed Library (Alice)
    var seeder = scope.ServiceProvider.GetRequiredService<LibrarySeeder>();
    await seeder.SeedLibraryAsync(); // Fire and forget in real production, but wait here for demo
}
// ========================================================

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseStaticFiles(); // Static files FIRST
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

