using Microsoft.EntityFrameworkCore;
using StoryFort.Models;

namespace StoryFort.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }


    public DbSet<Theme> Themes { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Story> Stories { get; set; }
    public DbSet<Notebook> Notebooks { get; set; }
    public DbSet<NotebookEntity> NotebookEntities { get; set; }
    public DbSet<NotebookEntry> NotebookEntries { get; set; }
    public DbSet<StoryEntityLink> StoryEntityLinks { get; set; }
    public DbSet<NotebookType> NotebookTypes { get; set; }
    public DbSet<Archetype> Archetypes { get; set; }
    public DbSet<ArchetypePoint> ArchetypePoints { get; set; }
    public DbSet<ArchetypeExample> ArchetypeExamples { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Many-to-Many Link Key
        modelBuilder.Entity<StoryEntityLink>()
            .HasKey(sl => new { sl.StoryId, sl.NotebookEntityId });

        // Load seed data from external JSON to keep OnModelCreating concise and editable.
        try
        {
            var seedPath = System.IO.Path.Combine(System.AppContext.BaseDirectory, "Data", "seed", "seeddata.json");
            if (!System.IO.File.Exists(seedPath))
            {
                // Fallback to repo-relative path during development
                seedPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "StoryFort", "Data", "seed", "seeddata.json");
            }

            if (System.IO.File.Exists(seedPath))
            {
                var json = System.IO.File.ReadAllText(seedPath);
                var opts = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var seed = System.Text.Json.JsonSerializer.Deserialize<SeedDto>(json, opts);

                if (seed?.Archetypes != null && seed.Archetypes.Count > 0)
                    modelBuilder.Entity<Archetype>().HasData(seed.Archetypes.ToArray());

                if (seed?.ArchetypePoints != null && seed.ArchetypePoints.Count > 0)
                    modelBuilder.Entity<ArchetypePoint>().HasData(seed.ArchetypePoints.ToArray());

                if (seed?.ArchetypeExamples != null && seed.ArchetypeExamples.Count > 0)
                    modelBuilder.Entity<ArchetypeExample>().HasData(seed.ArchetypeExamples.ToArray());

                if (seed?.NotebookTypes != null && seed.NotebookTypes.Count > 0)
                    modelBuilder.Entity<NotebookType>().HasData(seed.NotebookTypes.ToArray());

                if (seed?.Themes != null && seed.Themes.Count > 0)
                    modelBuilder.Entity<Theme>().HasData(seed.Themes.ToArray());

                if (seed?.Accounts != null && seed.Accounts.Count > 0)
                    modelBuilder.Entity<Account>().HasData(seed.Accounts.ToArray());
            }
        }
        catch
        {
            // Swallow seed loading errors to avoid blocking migrations; developers should run the seeder separately if needed.
        }
    }
}
