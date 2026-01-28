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
builder.Services.AddScoped<TutorOrchestrator>();
builder.Services.AddScoped<ICohereTutorService, CohereTutorService>();

// LLM Integration Service
builder.Services.AddHttpClient("LLM", client => 
{
    client.BaseAddress = new Uri("https://api.cohere.com/");
    client.Timeout = TimeSpan.FromMinutes(5); // LLMs can be slow
});

// Database (Future To-Do: AddDbContext<AppDbContext>(...))
// builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=StoryFort.db"));

var app = builder.Build();

// === PILOT HARDENING: Enable WAL Mode for Concurrency ===
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // Ensure created and migrate
    db.Database.Migrate();

    // Enable WAL Mode (Write-Ahead Logging) to allow concurrent readers/writers
    // This prevents SQLITE_BUSY errors when 30+ students save simultaneously.
    db.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
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

