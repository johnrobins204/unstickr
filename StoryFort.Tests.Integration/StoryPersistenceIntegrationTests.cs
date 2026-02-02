using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StoryFort.Data;
using StoryFort.Models;
using StoryFort.Services;
using Xunit;

namespace StoryFort.Tests.Integration;

public class StoryPersistenceIntegrationTests
{
    [Fact]
    public async Task LoadStoryAsync_Includes_Notebooks_Entities_Entries_And_Links()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var factory = new SimpleScopeFactory(options);

        // Seed data
        using (var s = factory.CreateScope())
        {
            var db = (AppDbContext?)s.ServiceProvider.GetService(typeof(AppDbContext));
            db!.Database.EnsureCreated();

            var acct = new Account { Name = "T" };
            var theme = new Theme { Name = "Default" };
            acct.ActiveTheme = theme;

            var notebook = new Notebook { Name = "N1", Account = acct };

            var entity = new NotebookEntity { Name = "Pins", Notebook = notebook };

            var story = new Story { Id = 11, Title = "S", Content = "<p>x</p>", Account = acct };

            var entry = new NotebookEntry { Content = "e", NotebookEntity = entity, Story = story };

            var link = new StoryEntityLink { Story = story, NotebookEntity = entity };

            db.Accounts.Add(acct);
            db.Notebooks.Add(notebook);
            db.NotebookEntities.Add(entity);
            db.Stories.Add(story);
            db.NotebookEntries.Add(entry);
            db.StoryEntityLinks.Add(link);

            db.SaveChanges();
        }

        var svc = new StoryPersistenceService(factory);

        var loaded = await svc.LoadStoryAsync(11);
        loaded.Should().NotBeNull();
        loaded!.Account.Should().NotBeNull();
        loaded.Account!.Notebooks.Should().NotBeNullOrEmpty();
        loaded.Account.Notebooks.Should().Contain(n => n.Name == "N1");
        loaded.Account.Notebooks.First().Entities.Should().Contain(e => e.Name == "Pins");
        loaded.EntityLinks.Should().NotBeNull();
        loaded.EntityLinks.Should().Contain(l => l.NotebookEntityId == loaded.Account.Notebooks.First().Entities.First().Id);

        connection.Close();
    }

    [Fact]
    public async Task SaveMethods_Handle_Missing_Story_Gracefully()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var factory = new SimpleScopeFactory(options);

        using (var s = factory.CreateScope())
        {
            var db = (AppDbContext?)s.ServiceProvider.GetService(typeof(AppDbContext));
            db!.Database.EnsureCreated();
        }

        var svc = new StoryPersistenceService(factory);

        // Should not throw when story does not exist
        await svc.SaveContentAsync(9999, "<p>a</p>", "t", "g");
        await svc.SaveMetadataAsync(9999, "{}");

        connection.Close();
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
