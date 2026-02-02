using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StoryFort.Data;
using StoryFort.Models;
using StoryFort.Services;
using Xunit;


namespace StoryFort.Tests.Unit;

public class StoryPersistenceServiceTests
{
    [Fact]
    public async Task SaveAndLoadContent_PersistsContent()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("testdb_content")
            .Options;

        // Use the same factory to seed the DB so the service sees the same data
        var factory = new SimpleScopeFactory(options);
        using (var s = factory.CreateScope())
        {
            var db = (AppDbContext?)s.ServiceProvider.GetService(typeof(AppDbContext));
            db!.Stories.Add(new Story { Id = 1, Title = "Initial", Content = "<p>hello</p>" });
            db.SaveChanges();
        }

        var svc = new StoryPersistenceService(factory);

        // Verify seeded story exists via direct context
        using (var check = factory.CreateScope())
        {
            var db = (AppDbContext?)check.ServiceProvider.GetService(typeof(AppDbContext));
            var story = await db!.Stories.FindAsync(1);
            story.Should().NotBeNull();
            story!.Content.Should().Be("<p>hello</p>");
        }

        await svc.SaveContentAsync(1, "<p>updated</p>", "NewTitle", "Fantasy");

        using (var check2 = factory.CreateScope())
        {
            var db = (AppDbContext?)check2.ServiceProvider.GetService(typeof(AppDbContext));
            var reloaded = await db!.Stories.FindAsync(1);
            reloaded.Should().NotBeNull();
            reloaded!.Content.Should().Be("<p>updated</p>");
            reloaded.Title.Should().Be("NewTitle");
            reloaded.Genre.Should().Be("Fantasy");
        }
    }

    [Fact]
    public async Task SaveAndLoadMetadata_PersistsMetadata()
    {
        var options2 = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("testdb_meta")
            .Options;

        var factory2 = new SimpleScopeFactory(options2);
        using (var s = factory2.CreateScope())
        {
            var db = (AppDbContext?)s.ServiceProvider.GetService(typeof(AppDbContext));
            db!.Stories.Add(new Story { Id = 2, Title = "MetaTest", Content = "" });
            db.SaveChanges();
        }

        var svc = new StoryPersistenceService(factory2);

        await svc.SaveMetadataAsync(2, "{\"foo\":\"bar\"}");
        var meta = await svc.LoadMetadataAsync(2);
        meta.Should().Be("{\"foo\":\"bar\"}");
    }

    [Fact]
    public async Task EnsureNotebookAndEntity_CreateAndAddEntry()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("testdb_notebook_entity")
            .Options;

        var factory = new SimpleScopeFactory(options);
        var svc = new StoryPersistenceService(factory);

        // Ensure notebook is created
        var nb = await svc.EnsureNotebookAsync(1, "Quick Pins", "bi-pin");
        nb.Should().NotBeNull();
        nb.Name.Should().Be("Quick Pins");

        // Ensure entity is created
        var entity = await svc.EnsureEntityAsync(nb.Id, "Pins");
        entity.Should().NotBeNull();
        entity.Name.Should().Be("Pins");

        // Seed a story so AddNotebookEntryAsync can link to it
        using (var s = factory.CreateScope())
        {
            var db = (AppDbContext?)s.ServiceProvider.GetService(typeof(AppDbContext));
            db!.Stories.Add(new Story { Id = 5, Title = "Seeded", Content = "" });
            db.SaveChanges();
        }

        // Add an entry linked to the seeded story, with a sentence id
        var entry = await svc.AddNotebookEntryAsync(nb.Id, 5, "pinned content", "Pins", 1, "sent-1");
        entry.Should().NotBeNull();

        // Verify entry persisted and story metadata updated
        using (var check = factory.CreateScope())
        {
            var db = (AppDbContext?)check.ServiceProvider.GetService(typeof(AppDbContext));
            var stored = await db!.NotebookEntries.FindAsync(entry.Id);
            stored.Should().NotBeNull();
            stored!.Content.Should().Be("pinned content");

            var story = await db!.Stories.FindAsync(5);
            story.Should().NotBeNull();
            story!.Metadata.Should().NotBeNullOrWhiteSpace();
            story.Metadata.Should().Contain("pinnedSentences");
        }
    }

// Minimal IServiceScopeFactory implementation for tests
internal class SimpleScopeFactory : IServiceScopeFactory
{
    private readonly DbContextOptions<AppDbContext> _options;
    public SimpleScopeFactory(DbContextOptions<AppDbContext> options) => _options = options;
    public IServiceScope CreateScope() => new SimpleScope(_options);
}

internal class SimpleScope : IServiceScope
{
    private readonly DbContextOptions<AppDbContext> _options;
    public SimpleScope(DbContextOptions<AppDbContext> options)
    {
        _options = options;
        ServiceProvider = new SimpleProvider(_options);
    }

    public IServiceProvider ServiceProvider { get; }
    public void Dispose() { }
}

internal class SimpleProvider : IServiceProvider
{
    private readonly DbContextOptions<AppDbContext> _options;
    public SimpleProvider(DbContextOptions<AppDbContext> options) => _options = options;
    public object? GetService(Type serviceType)
    {
        if (serviceType == typeof(AppDbContext)) return new AppDbContext(_options);
        if (serviceType == typeof(IStoryContentProtector)) return new PassThroughProtector();
        return null;
    }
}

internal class PassThroughProtector : IStoryContentProtector
{
    public string Protect(string content) => content;
    public string Unprotect(string protectedContent) => protectedContent;
}

}
