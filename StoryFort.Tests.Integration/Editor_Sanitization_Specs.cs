using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StoryFort.Data;
using StoryFort.Services;
using Xunit;

namespace StoryFort.Tests.Integration
{
    public class Editor_Sanitization_Specs : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly ServiceProvider _provider;

        public Editor_Sanitization_Specs()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(opts => opts.UseSqlite(_connection));
            services.AddScoped<StoryPersistenceService>();

            _provider = services.BuildServiceProvider();

            using var scope = _provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();

            // EnsureCreated() already seeds Themes from OnModelCreating's HasData
            // So we just need to add Account and Story
            
            // Add Account (assume Theme Id 1 exists from seed data)
            var account = new Models.Account { Id = 100, Name = "Test Account", ActiveThemeId = 1 };
            db.Accounts.Add(account);
            db.SaveChanges();

            // Add the Story with AccountId
            db.Stories.Add(new Models.Story {
                Id = 501,
                Title = "Seed",
                Content = "<p>Hello <strong>world</strong>!</p>",
                Created = DateTime.Now,
                AccountId = 100
            });
            db.SaveChanges();
        }

        [Fact]
        public async Task SaveContent_EscapesInjectedBodyTag()
        {
            using var scope = _provider.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<StoryPersistenceService>();

            var malicious = "<p>Hello <strong>UNIVERSE!</body></strong>!</p>";
            await svc.SaveContentAsync(501, malicious, null, null);

            var loaded = await svc.LoadStoryAsync(501);
            loaded.Should().NotBeNull();
            var persisted = loaded!.Content;

            persisted.Should().NotContain("</body>");
            persisted.Should().Contain("&lt;/body&gt;");
            persisted.Should().StartWith("<p>");
            persisted.Should().EndWith("</p>");
        }

        [Fact]
        public async Task SaveContent_RemovesScriptTags()
        {
            using var scope = _provider.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<StoryPersistenceService>();

            var malicious = "<p>Hi <script>alert(1)</script> there</p>";
            await svc.SaveContentAsync(501, malicious, null, null);

            var loaded = await svc.LoadStoryAsync(501);
            loaded.Should().NotBeNull();
            var persisted = loaded!.Content;

            persisted.Should().NotContain("<script");
            persisted.Should().NotContain("alert(1)");
            persisted.Should().StartWith("<p>");
            persisted.Should().EndWith("</p>");
        }

        public void Dispose()
        {
            _provider.Dispose();
            _connection.Dispose();
        }
    }
}
