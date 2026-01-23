using Unstickd.Components;
using Unstickd.Services;
using Unstickd.Data;
using Blazored.TextEditor;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Server.Circuits;

// 1. Setup Serilog ASAP
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/unstickd-.txt", rollingInterval: RollingInterval.Day)
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
    options.UseSqlite("Data Source=unstickd.db"));

// State Management
builder.Services.AddScoped<StoryState>();
builder.Services.AddScoped<TutorOrchestrator>();

// LLM Integration Service
builder.Services.AddHttpClient("LLM", client => 
{
    client.BaseAddress = new Uri("http://localhost:11434");
    client.Timeout = TimeSpan.FromMinutes(5); // LLMs can be slow
});

// Database (Future To-Do: AddDbContext<AppDbContext>(...))
// builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=unstickd.db"));

var app = builder.Build();

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
