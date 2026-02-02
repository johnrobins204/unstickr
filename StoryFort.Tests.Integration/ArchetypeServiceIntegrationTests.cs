using System;
using System.Linq;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StoryFort.Data;
using StoryFort.Models;
using StoryFort.Services;
using Xunit;

namespace StoryFort.Tests.Integration;

public class ArchetypeServiceIntegrationTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _provider;

    public ArchetypeServiceIntegrationTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(opts => opts.UseSqlite(_connection));
        services.AddScoped<ArchetypeService>();

        _provider = services.BuildServiceProvider();

        // Ensure DB created
        using var scope = _provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();

        // Seed minimal archetypes only if missing (app may have seeded already)
        if (!db.Archetypes.Any(a => a.Id == "hero")) db.Archetypes.Add(new Archetype { Id = "hero", Name = "Hero" });
        if (!db.Archetypes.Any(a => a.Id == "quest")) db.Archetypes.Add(new Archetype { Id = "quest", Name = "Quest" });
        if (!db.Archetypes.Any(a => a.Id == "transform")) db.Archetypes.Add(new Archetype { Id = "transform", Name = "Transform" });
        db.SaveChanges();
    }

    [Fact]
    public void GetArchetypes_LoadsSeededArchetypes()
    {
        using var scope = _provider.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<ArchetypeService>();
        var list = svc.GetArchetypes();
        list.Count.Should().BeGreaterThan(2);
        list.Select(a => a.Id).Should().Contain(new[] { "hero", "quest", "transform" });
    }

    public void Dispose()
    {
        _provider.Dispose();
        _connection.Dispose();
    }
}
