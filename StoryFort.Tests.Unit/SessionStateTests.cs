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

public class SessionStateTests
{
    [Fact]
    public void NotifyStateChanged_TriggersOnChangeEvent()
    {
        var session = new SessionState();
        var called = false;
        session.OnChange += () => called = true;

        session.NotifyStateChanged();

        called.Should().BeTrue();
    }

    [Fact]
    public void NotifyStateChanged_WithNoSubscribers_DoesNotThrow()
    {
        var session = new SessionState();
        Action act = () => session.NotifyStateChanged();
        act.Should().NotThrow();
    }

    [Fact]
    public void LoadNotebooks_CachesInMemory()
    {
        var session = new SessionState();
        session.Notebooks.Should().BeEmpty();
        session.Notebooks.Add(new Notebook { Name = "A", Id = 1 });
        session.Notebooks.Should().ContainSingle(n => n.Name == "A");
    }

    [Fact]
    public async Task UpdateThemePreference_PersistsToDatabase()
    {
        var services = new ServiceCollection();
        var dbName = Guid.NewGuid().ToString();
        services.AddDbContext<AppDbContext>(opts => opts.UseInMemoryDatabase(dbName));
        services.AddScoped<ThemeService>();
        var provider = services.BuildServiceProvider();

        // Seed an account
        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Accounts.Add(new Account { Name = "T", SupervisorEmail = "s@x" });
            db.SaveChanges();
        }

        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
        var session = new SessionState(scopeFactory);

        // update preference and pass account
        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var account = await db.Accounts.FirstAsync();

            session.UpdateThemePreference(tp => tp.FontSize = 20, account);

            // Wait for background persistence to run (poll using fresh scopes)
            var succeeded = false;
            for (int i = 0; i < 40; i++)
            {
                await Task.Delay(100);
                using var checkScope = provider.CreateScope();
                var checkDb = checkScope.ServiceProvider.GetRequiredService<AppDbContext>();
                var refreshed = await checkDb.Accounts.FindAsync(account.Id);
                if (refreshed != null && !string.IsNullOrWhiteSpace(refreshed.ThemePreferenceJson) && refreshed.ThemePreferenceJson != "{}")
                {
                    succeeded = true; break;
                }
            }

            succeeded.Should().BeTrue("ThemePreference should be persisted to the database by ThemeService");
        }
    }
}

