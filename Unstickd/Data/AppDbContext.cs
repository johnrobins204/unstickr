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

        // Seed Default Account
        modelBuilder.Entity<Account>().HasData(
            new Account { Id = 1, Name = "Writer" }
        );

        // Seed Archetypes
        modelBuilder.Entity<Archetype>().HasData(
            new Archetype 
            { 
                Id = "classic", 
                Name = "Classic Arc (Freytag)", 
                Description = "A classic rise and fall structure suitable for most stories.",
                SvgPath = "M0,320 L100,320 L300,50 L500,320 L800,320"
            },
            new Archetype 
            { 
                Id = "hero", 
                Name = "Hero's Journey", 
                Description = "A circular journey where the hero leaves home and returns changed.",
                SvgPath = "M0,320 L100,320 L250,150 L400,38 L550,150 L700,320 L800,320"
            },
            new Archetype 
            { 
                Id = "kisho", 
                Name = "The Twist (Kishōtenketsu)", 
                Description = "An East Asian structure with four acts: Intro, Development, Twist, and Conclusion.",
                SvgPath = "M0,320 L100,320 L300,200 L550,50 L750,320 L800,320"
            }
        );

        modelBuilder.Entity<ArchetypePoint>().HasData(
            // Classic
            new ArchetypePoint { Id = 1, ArchetypeId = "classic", StepId = 1, Label = "Beginning", Prompt = "How does the story start? Describe the normal world.", X = 50, Y = 308, Align = "center" },
            new ArchetypePoint { Id = 2, ArchetypeId = "classic", StepId = 2, Label = "Inciting Incident", Prompt = "What event changes everything for the hero?", X = 150, Y = 250, Align = "center" },
            new ArchetypePoint { Id = 3, ArchetypeId = "classic", StepId = 3, Label = "Rising Action", Prompt = "What obstacles does the hero face along the way?", X = 225, Y = 150, Align = "right" },
            new ArchetypePoint { Id = 4, ArchetypeId = "classic", StepId = 4, Label = "The Climax", Prompt = "The biggest battle or challenge!", X = 300, Y = 38, Align = "center" },
            new ArchetypePoint { Id = 5, ArchetypeId = "classic", StepId = 5, Label = "Falling Action", Prompt = "What happens immediately after the climax?", X = 400, Y = 185, Align = "left" },
            new ArchetypePoint { Id = 6, ArchetypeId = "classic", StepId = 6, Label = "Resolution", Prompt = "How does the story end? What is the new normal?", X = 650, Y = 308, Align = "center" },

            // Hero
            new ArchetypePoint { Id = 7, ArchetypeId = "hero", StepId = 1, Label = "Ordinary World", Prompt = "Describe the hero's life before the adventure.", X = 50, Y = 308, Align = "center" },
            new ArchetypePoint { Id = 8, ArchetypeId = "hero", StepId = 2, Label = "Call to Adventure", Prompt = "Who or what calls them to action?", X = 175, Y = 235, Align = "right" },
            new ArchetypePoint { Id = 9, ArchetypeId = "hero", StepId = 3, Label = "Threshold", Prompt = "The hero leaves home and enters the unknown.", X = 250, Y = 150, Align = "right" },
            new ArchetypePoint { Id = 10, ArchetypeId = "hero", StepId = 4, Label = "The Ordeal", Prompt = "The central crisis where the hero faces their greatest fear.", X = 400, Y = 38, Align = "center" },
            new ArchetypePoint { Id = 11, ArchetypeId = "hero", StepId = 5, Label = "The Road Back", Prompt = "The hero must return home with what they learned.", X = 550, Y = 150, Align = "left" },
            new ArchetypePoint { Id = 12, ArchetypeId = "hero", StepId = 6, Label = "Return w/ Elixir", Prompt = "The hero returns home, changed forever.", X = 700, Y = 308, Align = "center" },

            // Kisho
            new ArchetypePoint { Id = 13, ArchetypeId = "kisho", StepId = 1, Label = "Introduction (Ki)", Prompt = "Introduce the characters and their world.", X = 50, Y = 308, Align = "center" },
            new ArchetypePoint { Id = 14, ArchetypeId = "kisho", StepId = 2, Label = "Development (Shō)", Prompt = "Deepen the story. What are they doing? (No major conflict yet)", X = 200, Y = 250, Align = "center" },
            new ArchetypePoint { Id = 15, ArchetypeId = "kisho", StepId = 3, Label = "The Twist (Ten)", Prompt = "Surprise! Something unexpected happens that changes everything.", X = 550, Y = 50, Align = "center" },
            new ArchetypePoint { Id = 16, ArchetypeId = "kisho", StepId = 4, Label = "Conclusion (Ketsu)", Prompt = "How does the story settle after the twist?", X = 750, Y = 308, Align = "center" }
        );

        // Seed Notebook Types
        modelBuilder.Entity<NotebookType>().HasData(
            new NotebookType { Id = 1, Name = "Characters", Icon = "bi-person", IsSystemDefault = true },
            new NotebookType { Id = 2, Name = "Places", Icon = "bi-geo-alt", IsSystemDefault = true },
            new NotebookType { Id = 3, Name = "Spells", Icon = "bi-magic", IsSystemDefault = true },
            new NotebookType { Id = 4, Name = "Recipes", Icon = "bi-egg-fried", IsSystemDefault = true },
            new NotebookType { Id = 5, Name = "Creatures", Icon = "bi-bug", IsSystemDefault = true },
            new NotebookType { Id = 6, Name = "Items", Icon = "bi-box-seam", IsSystemDefault = true },
            new NotebookType { Id = 7, Name = "Lore", Icon = "bi-book", IsSystemDefault = true }
        );

        // Seed Archetype Examples (Wizard of Oz)
        modelBuilder.Entity<ArchetypeExample>().HasData(
            // Hero's Journey (Wizard of Oz)
            new ArchetypeExample { Id = 1, ArchetypePointId = 7, Title = "The Wizard of Oz", Content = "Dorothy lives on a gray, dry prairie in Kansas with Aunt Em and Uncle Henry. Her life is dull, and she dreams of 'Somewhere Over the Rainbow'." },
            new ArchetypeExample { Id = 2, ArchetypePointId = 8, Title = "The Wizard of Oz", Content = "The mean Miss Gulch takes Toto away with a sheriff's order to be destroyed. Toto escapes, but Dorothy decides she must run away to keep him safe." },
            new ArchetypeExample { Id = 3, ArchetypePointId = 9, Title = "The Wizard of Oz", Content = "A cyclone rips the farmhouse from the ground and deposits it in the colorful Land of Oz. Dorothy opens the door to a technicolor world, leaving the black-and-white Kansas behind." },
            new ArchetypeExample { Id = 4, ArchetypePointId = 10, Title = "The Wizard of Oz", Content = "Dorothy and her friends are captured by Flying Monkeys and trapped in the Wicked Witch's castle. The hour glass is running out!" },
            new ArchetypeExample { Id = 5, ArchetypePointId = 11, Title = "The Wizard of Oz", Content = "The Wizard offers to take Dorothy home in his balloon, but it accidentally launches while she is chasing Toto, leaving her stranded again." },
            new ArchetypeExample { Id = 6, ArchetypePointId = 12, Title = "The Wizard of Oz", Content = "Dorothy learns she had the power all along. She taps her ruby slippers three times, says 'There's no place like home', and wakes up in her own bed surrounded by her family." }
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
