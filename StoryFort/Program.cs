using StoryFort.Components;
using System.Linq;
using StoryFort.Api;
using StoryFort.Services;
using StoryFort.Data;
using Blazored.TextEditor;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Server.Circuits;

// 1. Setup Serilog ASAP with redaction enricher
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/StoryFort-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .Enrich.With(new RedactEnricher())
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

// Data Protection for API key encryption
builder.Services.AddDataProtection();
builder.Services.AddSingleton<IApiKeyProtector, ApiKeyProtector>();
builder.Services.AddSingleton<IStoryContentProtector, StoryContentProtector>();
// Bind prompt options
builder.Services.Configure<SparkPromptOptions>(builder.Configuration.GetSection("Prompts:Spark"));
builder.Services.Configure<SafeguardOptions>(builder.Configuration.GetSection("Safeguards"));

// State Management
builder.Services.AddScoped<SessionState>();
builder.Services.AddScoped<StoryContext>();
builder.Services.AddScoped<StoryPersistenceService>();
builder.Services.AddSingleton<ArchetypeService>();
builder.Services.AddSingleton<TextTokenizer>();
builder.Services.AddScoped<TutorOrchestrator>();
builder.Services.AddScoped<ICohereTutorService, CohereTutorService>();
builder.Services.AddScoped<SparkPromptStrategy>();
builder.Services.AddScoped<ReviewPromptStrategy>();
// Default IPromptStrategy implementation
builder.Services.AddScoped<IPromptStrategy, SparkPromptStrategy>();
builder.Services.AddScoped<ISafeguardService, SafeguardService>();
// Theme persistence service
builder.Services.AddScoped<ThemeService>();
// Tutor session service (extracted from StoryState)
builder.Services.AddScoped<TutorSessionService>();
// Prompt repository for loading prompt templates from disk (Prompts/)
builder.Services.AddSingleton<PromptRepository>();
// Prompt templating service
builder.Services.AddSingleton<IPromptService, PromptService>();
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

var app = builder.Build();

// Initialize static encryption provider for EF Core Value Converters
StoryFort.Services.StoryEncryptionProvider.Protector = app.Services.GetRequiredService<StoryFort.Services.IStoryContentProtector>();

// Minimal API for reader pinning + notebook lookup
// Move API endpoints to dedicated mapping extensions
app.MapStoryEndpoints();

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

    // Verification: log counts of seeded entities
    try
    {
        var archetypeCount = db.Archetypes.Count();
        var pointCount = db.ArchetypePoints.Count();
        var themeCount = db.Themes.Count();
        var accountCount = db.Accounts.Count();
        Log.Information("Seed verification: Archetypes={ArchetypeCount}, Points={PointCount}, Themes={ThemeCount}, Accounts={AccountCount}", archetypeCount, pointCount, themeCount, accountCount);
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Unable to verify seed counts");
    }
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

// Expose Program for WebApplicationFactory in integration tests
public partial class Program { }

