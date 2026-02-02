using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StoryFort.Data;
using StoryFort.Models;
using StoryFort.Services;
using Xunit;

namespace StoryFort.Tests.Unit;

public class AchievementServiceTests
{
    [Fact]
    public async Task UnlockBadgeAsync_AddsBadge_WhenNotPresent()
    {
        var services = new ServiceCollection();
        var dbName = Guid.NewGuid().ToString();
        services.AddDbContext<AppDbContext>(opts => opts.UseInMemoryDatabase(dbName));
        services.AddScoped<AchievementService>();
        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var acct = new Account { Name = "A" };
        db.Accounts.Add(acct);
        await db.SaveChangesAsync();

        var svc = scope.ServiceProvider.GetRequiredService<AchievementService>();
        var unlocked = await svc.UnlockBadgeAsync(acct.Id, "first_draft", "First Draft");
        unlocked.Should().BeTrue();

        var has = await svc.HasBadgeAsync(acct.Id, "first_draft");
        has.Should().BeTrue();
    }

    [Fact]
    public async Task UnlockBadgeAsync_ReturnsFalse_IfAlreadyHasBadge()
    {
        var services = new ServiceCollection();
        var dbName = Guid.NewGuid().ToString();
        services.AddDbContext<AppDbContext>(opts => opts.UseInMemoryDatabase(dbName));
        services.AddScoped<AchievementService>();
        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var acct = new Account { Name = "A", ThemePreferenceJson = "{ \"badges\": [\"x\"] }" };
        db.Accounts.Add(acct);
        await db.SaveChangesAsync();

        var svc = scope.ServiceProvider.GetRequiredService<AchievementService>();
        var unlocked = await svc.UnlockBadgeAsync(acct.Id, "x", "X");
        unlocked.Should().BeFalse();
    }
}

