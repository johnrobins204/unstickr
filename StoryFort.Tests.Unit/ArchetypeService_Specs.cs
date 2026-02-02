using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StoryFort.Data;
using StoryFort.Models;
using StoryFort.Services;
using Xunit;

namespace StoryFort.Tests.Unit
{
    /// <summary>
    /// Spec: /specs/archetype-service.md
    /// </summary>
    public class ArchetypeService_Specs : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly ServiceProvider _provider;

        public ArchetypeService_Specs()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var services = new ServiceCollection();
            services.AddSingleton(_connection);
            services.AddDbContext<AppDbContext>((sp, opts) => 
                opts.UseSqlite(sp.GetRequiredService<SqliteConnection>()));
            services.AddScoped<ArchetypeService>();
            _provider = services.BuildServiceProvider();

            using var scope = _provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        }

        [Fact]
        public void GetArchetypes_NoSeed_ReturnsEmptyList()
        {
            using var scope = _provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
            
            // Remove all seeded archetypes to simulate "no seed" scenario
            db.Archetypes.RemoveRange(db.Archetypes);
            db.SaveChanges();
            
            var svc = _provider.GetRequiredService<ArchetypeService>();
            var list = svc.GetArchetypes();
            list.Should().NotBeNull();
            list.Should().BeEmpty();
        }

        [Fact]
        public void GetArchetypeById_NullOrEmpty_ReturnsNull()
        {
            var svc = _provider.GetRequiredService<ArchetypeService>();
            svc.GetArchetypeById(null).Should().BeNull();
            svc.GetArchetypeById("").Should().BeNull();
            svc.GetArchetypeById("   ").Should().BeNull();
        }

        [Fact]
        public void GetArchetypeById_Nonexistent_ReturnsNull()
        {
            var svc = _provider.GetRequiredService<ArchetypeService>();
            svc.GetArchetypeById("does-not-exist").Should().BeNull();
        }

        [Fact]
        public void GetArchetypes_WithSeed_ReturnsNormalizedObjects()
        {
            using var scope = _provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            var archetype = new Archetype { Id = "test-arch1", Name = "Hero", Description = "Desc", SvgPath = "p" };
            var point = new ArchetypePoint { StepId = 1, Label = "Start", Prompt = "p" };
            archetype.Points = new List<ArchetypePoint> { point };
            db.Archetypes.Add(archetype);
            db.SaveChanges();

            var svc = _provider.GetRequiredService<ArchetypeService>();
            var list = svc.GetArchetypes();
            list.Should().NotBeEmpty();
            var a = list.FirstOrDefault(x => x.Id == "test-arch1");
            a.Should().NotBeNull();
            a!.Points.Should().NotBeNull();
            a.Points.Should().HaveCountGreaterThan(0);
        }

        [Fact]
        public void GetArchetypes_NormalizesNullExamples()
        {
            using var scope = _provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            var archetype = new Archetype { Id = "test-arch2", Name = "Simple" };
            var point = new ArchetypePoint { StepId = 2, Label = "X", Prompt = "Y", Examples = null };
            archetype.Points = new List<ArchetypePoint> { point };
            db.Archetypes.Add(archetype);
            db.SaveChanges();

            var svc = _provider.GetRequiredService<ArchetypeService>();
            var a = svc.GetArchetypes().FirstOrDefault(x => x.Id == "test-arch2");
            a.Should().NotBeNull();
            a!.Points.Should().NotBeEmpty();
            a.Points.First().Examples.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public void GetArchetypes_MalformedSeed_FallsBackToEmpty()
        {
            // Create a fake scope factory that throws when resolving AppDbContext
            var services = new ServiceCollection();
            services.AddScoped<IServiceScopeFactory, BrokenScopeFactory>();
            services.AddScoped<ArchetypeService>();
            var prov = services.BuildServiceProvider();

            var svc = prov.GetRequiredService<ArchetypeService>();
            var list = svc.GetArchetypes();
            list.Should().NotBeNull();
            list.Should().BeEmpty();
        }

        public void Dispose()
        {
            _provider.Dispose();
            _connection.Dispose();
        }

        private class BrokenScopeFactory : IServiceScopeFactory
        {
            public IServiceScope CreateScope()
            {
                return new BrokenScope();
            }
        }

        private class BrokenScope : IServiceScope
        {
            public IServiceProvider ServiceProvider => new BrokenProvider();
            public void Dispose() { }
        }

        private class BrokenProvider : IServiceProvider
        {
            public object? GetService(Type serviceType)
            {
                throw new Exception("Simulated seed parse failure");
            }
        }
    }
}
