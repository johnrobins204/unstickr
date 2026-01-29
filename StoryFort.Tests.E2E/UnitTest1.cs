using System.IO;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace StoryFort.Tests.E2E;

public class UnitTest1
{
    [Fact]
    public async Task PlannerPage_CaptureScreenshot()
    {
        var outDir = Path.Combine(Directory.GetCurrentDirectory(), "test-artifacts");
        Directory.CreateDirectory(outDir);

        using var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });

        // Ensure the app is running; if not, start it for the duration of the test
        bool serverUp = false;
        try
        {
            using var wc = new System.Net.Http.HttpClient();
            wc.Timeout = System.TimeSpan.FromSeconds(1);
            var res = await wc.GetAsync("http://localhost:5112/planner/1");
            serverUp = res.IsSuccessStatusCode;
        }
        catch { serverUp = false; }

        System.Diagnostics.Process? serverProcess = null;
        if (!serverUp)
        {
            var psi = new System.Diagnostics.ProcessStartInfo("dotnet", "run --project \"C:\\Users\\johnr\\OneDrive\\Documents\\StoryFort\\StoryFort\\StoryFort.csproj\"")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            serverProcess = System.Diagnostics.Process.Start(psi);

            // Poll until server responds or timeout
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.Elapsed.TotalSeconds < 20)
            {
                try
                {
                    using var wc = new System.Net.Http.HttpClient();
                    wc.Timeout = System.TimeSpan.FromSeconds(1);
                    var res = await wc.GetAsync("http://localhost:5112/planner/1");
                    if (res.IsSuccessStatusCode) { serverUp = true; break; }
                }
                catch { }
                await Task.Delay(250);
            }
        }
        var page = await browser.NewPageAsync();

        await page.GotoAsync("http://localhost:5112/planner/1", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForSelectorAsync("svg");

        var svg = await page.QuerySelectorAsync("svg");
        var beforePath = Path.Combine(outDir, "planner_before.png");
        var hoverPath = Path.Combine(outDir, "planner_hover.png");
        var dataPath = Path.Combine(outDir, "planner_measure.json");

        if (svg == null)
        {
            await page.ScreenshotAsync(new PageScreenshotOptions { Path = beforePath, FullPage = true });
            throw new Xunit.Sdk.XunitException("SVG not found on Planner page");
        }

        // Capture full svg screenshot
        await svg.ScreenshotAsync(new ElementHandleScreenshotOptions { Path = beforePath });

        // Find first point group (we add class 'map-point')
        var point = await page.QuerySelectorAsync("svg g.map-point");
        if (point == null)
        {
            // fallback: any group
            point = await page.QuerySelectorAsync("svg g");
        }

        var beforeBox = await point.BoundingBoxAsync();

        // Simulate hover by applying a scale transform via JS (avoids pointer interception)
        await point.EvaluateAsync("(el) => { el.style.transition = 'transform 0.18s ease-in-out'; el.style.transformOrigin = 'center'; el.style.transform = 'scale(1.10)'; }");
        await page.WaitForTimeoutAsync(200);
        await svg.ScreenshotAsync(new ElementHandleScreenshotOptions { Path = hoverPath });
        var afterBox = await point.BoundingBoxAsync();

        // Save measurements
        var measurements = new
        {
            before = beforeBox,
            after = afterBox,
            beforeImage = beforePath,
            hoverImage = hoverPath
        };
        var json = System.Text.Json.JsonSerializer.Serialize(measurements, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(dataPath, json);

        Assert.True(File.Exists(beforePath));
        Assert.True(File.Exists(hoverPath));
        Assert.True(File.Exists(dataPath));

        await browser.CloseAsync();

        if (serverProcess != null && !serverProcess.HasExited)
        {
            try { serverProcess.Kill(true); } catch { }
        }
    }
}

