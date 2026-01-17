using Microsoft.EntityFrameworkCore;
using Unstickd.Models;

namespace Unstickd.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Theme> Themes { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Story> Stories { get; set; }
    public DbSet<StoryPage> Pages { get; set; }
    public DbSet<Notebook> Notebooks { get; set; }
    public DbSet<NotebookEntity> NotebookEntities { get; set; }
    public DbSet<NotebookEntry> NotebookEntries { get; set; }
    public DbSet<StoryEntityLink> StoryEntityLinks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Many-to-Many Link Key
        modelBuilder.Entity<StoryEntityLink>()
            .HasKey(sl => new { sl.StoryId, sl.NotebookEntityId });

        // Seed Default Account
        modelBuilder.Entity<Account>().HasData(
            new Account { Id = 1, Name = "Writer" }
        );

        // Seed 8 Classic Literature Themes
        modelBuilder.Entity<Theme>().HasData(
            new Theme { Id = 1, Name = "Wonderland", PrimaryColor = "#eec4d5", SecondaryColor = "#ba0d2d", FontName = "Alice", Description = "The Queen's Rose Garden", BackgroundTexture = "white", SpritePath = "images/cheshire.png" },
            new Theme { Id = 2, Name = "Secret Garden", PrimaryColor = "#8fb35d", SecondaryColor = "#5d8aa8", FontName = "Goudy Bookletter 1911", Description = "The Hidden Moorland Oasis", BackgroundTexture = "#f0f0f0", SpritePath = "images/robin.png" },
            new Theme { Id = 3, Name = "Neverland", PrimaryColor = "#191970", SecondaryColor = "#ffd700", FontName = "Cinzel Decorative", Description = "Neverland at Twilight", BackgroundTexture = "#0d1b2a", SpritePath = "images/pan.png" },
            new Theme { Id = 4, Name = "Oz", PrimaryColor = "#50c878", SecondaryColor = "#daa520", FontName = "Rye", Description = "The Emerald City", BackgroundTexture = "#fafff0", SpritePath = "images/toto.png" },
            new Theme { Id = 5, Name = "Treasure Island", PrimaryColor = "#d2b48c", SecondaryColor = "#000080", FontName = "Pirata One", Description = "Sun-Bleached Map", BackgroundTexture = "#f5deb3", SpritePath = "images/parrot.png" },
            new Theme { Id = 6, Name = "Riverbank", PrimaryColor = "#8b4513", SecondaryColor = "#9dc183", FontName = "WindSong", Description = "Wind in the Willows", BackgroundTexture = "#fdf5e6", SpritePath = "images/mole.png" },
            new Theme { Id = 7, Name = "Nursery", PrimaryColor = "#f5f5dc", SecondaryColor = "#b76e79", FontName = "Sniglet", Description = "The Velveteen Rabbit", BackgroundTexture = "#fff0f5", SpritePath = "images/rabbit.png" },
            new Theme { Id = 8, Name = "Workshop", PrimaryColor = "#deb887", SecondaryColor = "#6495ed", FontName = "Geostar Fill", Description = "Geppetto's Workshop", BackgroundTexture = "#fff8dc", SpritePath = "images/cricket.png" }
        );
    }
}
