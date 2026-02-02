using System.Text.Json;
using Serilog;
using StoryFort.Data;
using StoryFort.Models;
using Microsoft.Extensions.DependencyInjection;

namespace StoryFort.Services;

public class ThemeService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ThemeService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task SaveThemePreferenceAsync(int accountId, ThemePreference pref)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var account = await db.Accounts.FindAsync(accountId);
            if (account != null)
            {
                account.ThemePreferenceJson = JsonSerializer.Serialize(pref);
                await db.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error saving theme preference for Account {AccountId}", accountId);
        }
    }
}
