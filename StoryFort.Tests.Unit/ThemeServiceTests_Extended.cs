using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using StoryFort.Data;
using StoryFort.Models;
using StoryFort.Services;
using Xunit;

namespace StoryFort.Tests.Unit;

public class ThemeServiceTests_Extended
{
    [Fact]
    public async Task SaveThemePreference_SerializesToAccountJson()
    {
        var services = new ServiceCollection();
        var dbName = Guid.NewGuid().ToString();
        services.AddDbContext<AppDbContext>(opts => opts.UseInMemoryDatabase(dbName));
        services.AddScoped<ThemeService>();
        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var account = new Account { Name = "T" };
        db.Accounts.Add(account);
        await db.SaveChangesAsync();

        // Resolve ThemeService in a new scope to mirror runtime usage
        var pref = new ThemePreference { FontSize = 22, Primary = "#abc" };
        using (var scope2 = provider.CreateScope())
        {
            var svc = scope2.ServiceProvider.GetRequiredService<ThemeService>();
            await svc.SaveThemePreferenceAsync(account.Id, pref);
        }

        using var checkScope = provider.CreateScope();
        var refreshed = await checkScope.ServiceProvider.GetRequiredService<AppDbContext>().Accounts.FindAsync(account.Id);
        refreshed.Should().NotBeNull();
        refreshed!.ThemePreferenceJson.Should().Contain("Primary");
        refreshed.ThemePreference.FontSize.Should().Be(22);
    }
}
