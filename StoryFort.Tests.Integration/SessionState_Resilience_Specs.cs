using System;
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

public class SessionState_Resilience_Specs : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _provider;

    public SessionState_Resilience_Specs()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(opts => opts.UseSqlite(_connection));
        services.AddScoped<ThemeService>();
        services.AddScoped<SessionState>();

        _provider = services.BuildServiceProvider();

        using var scope = _provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();

        // seed account
        db.Accounts.Add(new Account { Id = 100, Name = "T", SupervisorEmail = "s@x" });
        db.SaveChanges();
    }

    [Fact]
    public async Task UpdateThemePreference_PersistsToDatabase()
    {
        var scopeFactory = _provider.GetRequiredService<IServiceScopeFactory>();
        var session = new SessionState(scopeFactory);

        using (var scope = _provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var account = await db.Accounts.FirstAsync(a => a.Id == 100);

            session.UpdateThemePreference(tp => tp.FontSize = 20, account);

            // poll for persistence
            var succeeded = false;
            for (int i = 0; i < 40; i++)
            {
                await Task.Delay(100);
                using var checkScope = _provider.CreateScope();
                var checkDb = checkScope.ServiceProvider.GetRequiredService<AppDbContext>();
                var refreshed = await checkDb.Accounts.FindAsync(account.Id);
                if (refreshed != null && !string.IsNullOrWhiteSpace(refreshed.ThemePreferenceJson) && refreshed.ThemePreferenceJson.Contains("\"FontSize\":20"))
                {
                    succeeded = true; break;
                }
            }

            succeeded.Should().BeTrue();
        }
    }

    [Fact]
    public async Task UpdateThemePreference_WithFailingThemeService_DoesNotThrowAndDoesNotPersist()
    {
        // Replace AppDbContext with one that fails on SaveChanges to simulate DB failure
        var services = new ServiceCollection();
        // register normal AppDbContext options (not used directly), then override AppDbContext
        services.AddDbContext<AppDbContext>(opts => opts.UseSqlite(_connection));
        services.AddScoped<AppDbContext>(sp => new FailingDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options));
        services.AddScoped<ThemeService>();
        services.AddScoped<SessionState>();
        var provider = services.BuildServiceProvider();

        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
        var session = new SessionState(scopeFactory);

        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var account = await db.Accounts.FirstAsync(a => a.Id == 100);

            Action act = () => session.UpdateThemePreference(tp => tp.FontSize = 25, account);
            act.Should().NotThrow();

            // wait briefly to allow background task attempt
            await Task.Delay(500);

            // verify DB not updated to 25
            using var checkScope = provider.CreateScope();
            var checkDb = checkScope.ServiceProvider.GetRequiredService<AppDbContext>();
            var refreshed = await checkDb.Accounts.FindAsync(account.Id);
            refreshed!.ThemePreferenceJson.Should().NotContain("\"FontSize\":25");
        }
    }

    [Fact]
    public void UpdateThemePreference_WithoutScopeFactory_DoesNotAttemptPersistence()
    {
        var session = new SessionState();
        session.ThemePreference.FontSize = 12;

        Action act = () => session.UpdateThemePreference(tp => tp.FontSize = 30, new Account { Id = 200 });
        act.Should().NotThrow();
        session.ThemePreference.FontSize.Should().Be(30);
    }

    [Fact]
    public async Task ConcurrentUpdates_LastWriteWins()
    {
        var scopeFactory = _provider.GetRequiredService<IServiceScopeFactory>();
        var session = new SessionState(scopeFactory);

        using (var scope = _provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var account = await db.Accounts.FirstAsync(a => a.Id == 100);

            // Apply update A then B
            session.UpdateThemePreference(tp => tp.FontSize = 40, account);
            session.UpdateThemePreference(tp => tp.FontSize = 45, account);

            // wait for persistence
            var succeeded = false;
            for (int i = 0; i < 40; i++)
            {
                await Task.Delay(100);
                using var checkScope = _provider.CreateScope();
                var checkDb = checkScope.ServiceProvider.GetRequiredService<AppDbContext>();
                var refreshed = await checkDb.Accounts.FindAsync(account.Id);
                if (refreshed != null && !string.IsNullOrWhiteSpace(refreshed.ThemePreferenceJson) && refreshed.ThemePreferenceJson.Contains("\"FontSize\":45"))
                {
                    succeeded = true; break;
                }
            }

            succeeded.Should().BeTrue();
        }
    }

    public void Dispose()
    {
        try
        {
            _provider?.Dispose();
        }
        catch { }
        try
        {
            _connection?.Dispose();
        }
        catch { }
    }

        private class FailingDbContext : AppDbContext
        {
            public FailingDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

            public override Task<int> SaveChangesAsync(System.Threading.CancellationToken cancellationToken = default)
            {
                throw new Exception("DB failure simulated");
            }
        }
}
