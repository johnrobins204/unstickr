using System;
using System.IO;
using FluentAssertions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.FileProviders;
using StoryFort.Services;
using Xunit;

namespace StoryFort.Tests.Unit;

internal class TestHostEnv : IHostEnvironment
{
    public string EnvironmentName { get; set; } = "Development";
    public string ApplicationName { get; set; } = "StoryFort.Tests";
    public string ContentRootPath { get; set; }
    public IFileProvider ContentRootFileProvider
    {
        get => new PhysicalFileProvider(ContentRootPath ?? string.Empty);
        set { }
    }
}

public class PromptServiceTests : IDisposable
{
    private readonly string _tmpDir;

    public PromptServiceTests()
    {
        _tmpDir = Path.Combine(Path.GetTempPath(), "prompts_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tmpDir);
        Directory.CreateDirectory(Path.Combine(_tmpDir, "Prompts"));
    }

    [Fact]
    public void FormatTemplate_ReplacesPlaceholders_AndRemovesUnmatched()
    {
        var key = "testprompt";
        var promptsDir = Path.Combine(_tmpDir, "Prompts");
        var file = Path.Combine(promptsDir, key + ".txt");
        File.WriteAllText(file, "Hello {{Name}}! Age: {{Age}}. {{UnusedPlaceholder}} end.");

        var env = new TestHostEnv { ContentRootPath = _tmpDir };
        var repo = new PromptRepository(env);
        var svc = new PromptService(repo);

        var formatted = svc.FormatTemplate(key, new System.Collections.Generic.Dictionary<string, string?>
        {
            ["Name"] = "Alex",
            ["Age"] = "9"
        });

        formatted.Should().NotBeNull();
        formatted.Should().Contain("Hello Alex");
        formatted.Should().Contain("Age: 9");
        formatted.Should().NotContain("UnusedPlaceholder");
    }

    public void Dispose()
    {
        try { Directory.Delete(_tmpDir, true); } catch { }
    }
}

