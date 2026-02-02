using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Xunit;

namespace StoryFort.Tests.E2E;

public class E2ETests : IDisposable
{
    private Process? _appProcess;
    private readonly HttpClient _http = new();
    private const string BaseUrl = "http://localhost:5112";

    private async Task StartAppAsync()
    {
        // 1. Locate the .csproj relative to this test assembly
        //    We are likely in StoryFort.Tests.E2E/bin/Debug/net10.0/
        var solutionRoot = FindSolutionRoot(AppContext.BaseDirectory);
        var projectPath = Path.Combine(solutionRoot, "StoryFort", "StoryFort.csproj");

        if (!File.Exists(projectPath))
            throw new FileNotFoundException($"Could not find project at {projectPath}");

        Console.WriteLine($"Starting app from: {projectPath}");

        // 2. Start the process
        //    IMPORTANT: Do NOT redirect Output/Error unless you consume them, 
        //    otherwise the buffer fills and the process hangs (deadlock).
        var psi = new ProcessStartInfo("dotnet", $"run --project \"{projectPath}\" --urls={BaseUrl}")
        {
            RedirectStandardOutput = false, // Allow output to flow to console (or nowhere)
            RedirectStandardError = false,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = solutionRoot // Run from root so appsettings etc are found if needed
        };

        try 
        {
            _appProcess = Process.Start(psi);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to start dotnet run: {ex.Message}");
        }

        if (_appProcess == null || _appProcess.HasExited)
            throw new Exception("Process failed to start immediately.");

        // 3. Wait for the server to be healthy
        var max = DateTime.UtcNow.AddSeconds(60);
        while (DateTime.UtcNow < max)
        {
            try
            {
                var resp = await _http.GetAsync(BaseUrl);
                if (resp.IsSuccessStatusCode) 
                {
                    Console.WriteLine("App is healthy!");
                    return;
                }
            }
            catch 
            {
                // App not ready yet
            }

            if (_appProcess.HasExited)
             throw new Exception($"App process exited unexpectedly with code {_appProcess.ExitCode}");

            await Task.Delay(1000);
        }

        // If we get here, we timed out
        KillProcess();
        throw new Exception("App did not start in time (60s).");
    }

    private static string FindSolutionRoot(string startPath)
    {
        var dir = new DirectoryInfo(startPath);
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "StoryFort.slnx")))
                return dir.FullName;
            dir = dir.Parent;
        }
        // Fallback: assume we are in <root>/StoryFort.Tests.E2E/bin...
        // So go up 4 levels (bin -> debug -> net -> project -> root) ??
        // Actually, just returning the passed path if not found allows error to bubble up naturally
        return startPath;
    }

    [Fact]
    public async Task HomePageLoadsAndShowsEditorPlaceholder()
    {
        await StartAppAsync();

        using var playwright = await Playwright.CreateAsync();
        // Set Headless = false if you want to see it visually on local dev
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        
        var page = await browser.NewPageAsync();
        
        // Go to Home
        await page.GotoAsync(BaseUrl);
        
        // Verify title or some element
        await Assertions.Expect(page).ToHaveTitleAsync(new System.Text.RegularExpressions.Regex("Reading Nook|StoryFort"));
        
        // Check for "Story Quest" or "Reading Nook" - current visible branding
        var body = page.Locator("body");
        await Assertions.Expect(body).ToContainTextAsync("Story Quest");
        
        await browser.CloseAsync();
    }

    public void Dispose()
    {
        KillProcess();
        _http.Dispose();
    }

    private void KillProcess()
    {
        if (_appProcess != null && !_appProcess.HasExited)
        {
            try { _appProcess.Kill(entireProcessTree: true); } catch { }
            _appProcess.Dispose();
            _appProcess = null;
        }
    }
}
