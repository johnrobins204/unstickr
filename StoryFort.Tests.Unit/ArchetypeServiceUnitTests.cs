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

public class ArchetypeServiceUnitTests
{
    [Fact]
    public void GetArchetypes_ReturnsPointsOrderedAndExamplesMapped()
    {
        var services = new ServiceCollection();
        var dbName = Guid.NewGuid().ToString();
        services.AddDbContext<AppDbContext>(opts => opts.UseInMemoryDatabase(dbName));
        services.AddScoped<ArchetypeService>();
        var provider = services.BuildServiceProvider();

        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var arch = new Archetype { Id = "hero", Name = "Hero", Description = "Desc", SvgPath = "/hero.svg" };
            var p2 = new ArchetypePoint { Archetype = arch, StepId = 2, Label = "Middle", Prompt = "Mid" };
            var p1 = new ArchetypePoint { Archetype = arch, StepId = 1, Label = "Start", Prompt = "Start" };
            p1.Examples.Add(new ArchetypeExample { Title = "Ex1", Content = "C1" });
            arch.Points.Add(p2);
            arch.Points.Add(p1);
            db.Archetypes.Add(arch);
            db.SaveChanges();
        }

        using (var scope2 = provider.CreateScope())
        {
            var svc = scope2.ServiceProvider.GetRequiredService<ArchetypeService>();
            var list = svc.GetArchetypes();
            list.Should().NotBeNullOrEmpty();
            var hero = list.First(a => a.Id == "hero");
            hero.Points.Should().NotBeNullOrEmpty();
            // Points should be ordered by StepId ascending
            hero.Points.Select(p => p.Id).Should().ContainInOrder(new[] { 1, 2 });
            // Examples mapped
            var start = hero.Points.First(p => p.Id == 1);
            start.Examples.Should().ContainSingle(e => e.Title == "Ex1");
        }
    }
}
