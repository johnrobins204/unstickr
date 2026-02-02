using System;
using System.Linq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StoryFort.Data;
using StoryFort.Models;
using StoryFort.Services;
using Xunit;

namespace StoryFort.Tests.Unit;

public class ArchetypeServiceTests
{
    [Fact]
    public void GetArchetypes_ReturnsMappedDefinitions()
    {
        var services = new ServiceCollection();
        var dbName = Guid.NewGuid().ToString();
        services.AddDbContext<AppDbContext>(opts => opts.UseInMemoryDatabase(dbName));
        services.AddScoped<ArchetypeService>();
        var provider = services.BuildServiceProvider();

        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var arch = new Archetype { Id = "hero", Name = "Hero", Description = "Heroic journey", SvgPath = "/hero.svg" };
            db.Archetypes.Add(arch);
            db.SaveChanges();
        }

        using (var scope2 = provider.CreateScope())
        {
            var svc = scope2.ServiceProvider.GetRequiredService<ArchetypeService>();

            var list = svc.GetArchetypes();
            list.Should().NotBeNullOrEmpty();
            var hero = list.FirstOrDefault(a => a.Id == "hero");
            hero.Should().NotBeNull();
        }
    }
}
