using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using StoryFort.Data;

namespace StoryFort.Services;

public class AchievementService
{
    private readonly AppDbContext _db;

    public AchievementService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> UnlockBadgeAsync(int accountId, string badgeKey, string badgeName)
    {
        var acct = await _db.Accounts.FindAsync(accountId);
        if (acct == null) return false;

        JsonObject root;
        try
        {
            root = string.IsNullOrWhiteSpace(acct.ThemePreferenceJson) ? new JsonObject() : JsonNode.Parse(acct.ThemePreferenceJson) as JsonObject ?? new JsonObject();
        }
        catch
        {
            root = new JsonObject();
        }

        var badges = root["badges"] as JsonArray;
        if (badges == null)
        {
            badges = new JsonArray();
            root["badges"] = badges;
        }

        if (badges.Select(n => n?.ToString()).Any(v => v == badgeKey)) return false; // already unlocked

        badges.Add(badgeKey);
        acct.ThemePreferenceJson = root.ToJsonString();
        _db.Accounts.Update(acct);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HasBadgeAsync(int accountId, string badgeKey)
    {
        var acct = await _db.Accounts.FindAsync(accountId);
        if (acct == null) return false;
        try
        {
            var root = string.IsNullOrWhiteSpace(acct.ThemePreferenceJson) ? null : JsonNode.Parse(acct.ThemePreferenceJson) as JsonObject;
            var badges = root?["badges"] as JsonArray;
            if (badges == null) return false;
            return badges.Select(n => n?.ToString()).Any(v => v == badgeKey);
        }
        catch { return false; }
    }
}
