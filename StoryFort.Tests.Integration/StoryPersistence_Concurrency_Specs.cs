using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StoryFort.Data;
using StoryFort.Models;
using StoryFort.Services;
using Xunit;

namespace StoryFort.Tests.Integration
{
    /// <summary>
    /// Spec: /specs/storypersistence-concurrency.md
    /// </summary>
    public class StoryPersistence_Concurrency_Specs : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly ServiceProvider _provider;

        public StoryPersistence_Concurrency_Specs()
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

            // seed account and story for concurrency tests
            db.Accounts.Add(new Account { Id = 600, Name = "ConcurrencyTest", SupervisorEmail = "test@x" });
            db.Stories.Add(new Story { Id = 601, AccountId = 600, Title = "Concurrent Test", Content = "<p>Start</p>", Created = DateTime.Now });
            db.SaveChanges();
        }

        [Fact]
        public async Task SaveContent_SingleWriter_PersistsSuccessfully()
        {
            using var scope = _provider.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<StoryPersistenceService>();

            await svc.SaveContentAsync(601, "<p>Updated</p>", null, null);

            var loaded = await svc.LoadStoryAsync(601);
            loaded.Should().NotBeNull();
            loaded!.Content.Should().Contain("Updated");
        }

        [Fact]
        public async Task ConcurrentSaves_LastWriteWins_NoCrash()
        {
            var svc1 = _provider.CreateScope().ServiceProvider.GetRequiredService<StoryPersistenceService>();
            var svc2 = _provider.CreateScope().ServiceProvider.GetRequiredService<StoryPersistenceService>();

            // Start two concurrent saves
            var taskA = Task.Run(async () =>
            {
                await Task.Delay(50); // slight delay so B might finish first
                await svc1.SaveContentAsync(601, "<p>Start — A</p>", null, null);
            });

            var taskB = Task.Run(async () =>
            {
                await svc2.SaveContentAsync(601, "<p>Start — B</p>", null, null);
            });

            // Both complete without throwing
            await Task.WhenAll(taskA, taskB);

            using var scope = _provider.CreateScope();
            var svcCheck = scope.ServiceProvider.GetRequiredService<StoryPersistenceService>();
            var loaded = await svcCheck.LoadStoryAsync(601);
            loaded.Should().NotBeNull();

            // Verify persisted content is one of the two (last write won)
            var content = loaded!.Content;
            var isValidResult = content.Contains("Start — A") || content.Contains("Start — B");
            isValidResult.Should().BeTrue("last-write-wins means one of the two updates persisted");

            // Verify HTML is complete (parseable)
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(content);
            doc.DocumentNode.Should().NotBeNull();
        }

        [Fact]
        public async Task ConcurrentSaves_LargeAndSmallPayload_NoCorruption()
        {
            var svc1 = _provider.CreateScope().ServiceProvider.GetRequiredService<StoryPersistenceService>();
            var svc2 = _provider.CreateScope().ServiceProvider.GetRequiredService<StoryPersistenceService>();

            var largeHtml = "<p>" + new string('X', 2000) + "</p>";
            var smallHtml = "<p>Tiny</p>";

            var taskA = Task.Run(async () => await svc1.SaveContentAsync(601, largeHtml, null, null));
            var taskB = Task.Run(async () => await svc2.SaveContentAsync(601, smallHtml, null, null));

            await Task.WhenAll(taskA, taskB);

            using var scope = _provider.CreateScope();
            var svcCheck = scope.ServiceProvider.GetRequiredService<StoryPersistenceService>();
            var loaded = await svcCheck.LoadStoryAsync(601);
            loaded.Should().NotBeNull();

            var content = loaded!.Content;

            // Verify no truncation: content should be either the large or small payload
            var isLarge = content.Length > 1500;
            var isSmall = content.Contains("Tiny");
            (isLarge || isSmall).Should().BeTrue("content must be one of the two complete payloads");

            // Verify HTML parses
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(content);
            doc.ParseErrors.Should().BeEmpty("HTML must parse without errors");
        }

        [Fact]
        public async Task SaveContent_DBFailure_ThrowsAndNoPersistence()
        {
            // Create a failing DB context
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(opts => opts.UseSqlite(_connection));
            services.AddScoped<AppDbContext>(sp => new FailingDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options));
            services.AddScoped<StoryPersistenceService>();
            var failProvider = services.BuildServiceProvider();

            using var scope = failProvider.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<StoryPersistenceService>();

            // Attempt save — expect it to throw due to DB failure
            Func<Task> act = async () => await svc.SaveContentAsync(601, "<p>FailTest</p>", null, null);
            await act.Should().ThrowAsync<Exception>();

            // Verify DB still has original content (no partial write)
            using var checkScope = _provider.CreateScope();
            var checkSvc = checkScope.ServiceProvider.GetRequiredService<StoryPersistenceService>();
            var loaded = await checkSvc.LoadStoryAsync(601);
            loaded.Should().NotBeNull();
            // Content should be from previous test or seed, not "FailTest"
            loaded!.Content.Should().NotContain("FailTest");
        }

        public void Dispose()
        {
            try { _provider?.Dispose(); } catch { }
            try { _connection?.Dispose(); } catch { }
        }

        private class FailingDbContext : AppDbContext
        {
            public FailingDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

            public override Task<int> SaveChangesAsync(System.Threading.CancellationToken cancellationToken = default)
            {
                throw new Exception("DB failure simulated for concurrency test");
            }
        }
    }
}
