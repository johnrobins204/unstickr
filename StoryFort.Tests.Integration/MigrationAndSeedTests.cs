using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using StoryFort.Data;

namespace StoryFort.Tests.Integration;

public class MigrationAndSeedTests
{
    [Fact]
    public async Task MigrationsApply_CreateExpectedTables()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        // Apply migrations against an in-memory SQLite instance
        using (var ctx = new AppDbContext(options))
        {
            await ctx.Database.MigrateAsync();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = 
                @"SELECT name FROM sqlite_master WHERE type='table' AND name IN ('Stories','Notebooks','NotebookEntities','NotebookEntries','Themes')";

            using var reader = await cmd.ExecuteReaderAsync();
            var found = 0;
            while (await reader.ReadAsync()) found++;

            Assert.True(found >= 1, $"Expected core tables to exist after migrations; found {found} matching tables.");
        }
    }
}
