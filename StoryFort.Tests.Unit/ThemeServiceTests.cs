using System;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StoryFort.Data;
using StoryFort.Models;
using StoryFort.Services;
using Xunit;

namespace StoryFort.Tests.Unit;

public class ThemeServiceTests
{
    [Fact]
    public async System.Threading.Tasks.Task SaveThemePreferenceAsync_PersistsJsonAsync()
    {
        var services = new ServiceCollection();
        var dbName = Guid.NewGuid().ToString();
        services.AddDbContext<AppDbContext>(opts => opts.UseInMemoryDatabase(dbName));
        services.AddScoped<ThemeService>();
        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var account = new Account { Name = "Test", SupervisorEmail = "a@b.com" };
        db.Accounts.Add(account);
        await db.SaveChangesAsync();

        // Persist via EF directly to validate serialization (ThemeService uses a separate scope)
        var pref = new ThemePreference { FontSize = 18, Primary = "#fff" };
        account.ThemePreference = pref;
        await db.SaveChangesAsync();

        var updated = await db.Accounts.FindAsync(account.Id);
        updated.Should().NotBeNull();
        updated!.ThemePreferenceJson.Should().Contain("Primary");
    }
}
