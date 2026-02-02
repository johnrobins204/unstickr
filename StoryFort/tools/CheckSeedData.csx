using Microsoft.EntityFrameworkCore;
using StoryFort.Data;
using Microsoft.Extensions.Logging;

var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
optionsBuilder.UseSqlite("Data Source=StoryFort/StoryFort.db");
optionsBuilder.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()));

using var db = new AppDbContext(optionsBuilder.Options);

var archetypeCount = db.Archetypes.Count();
var pointCount = db.ArchetypePoints.Count();
var exampleCount = db.ArchetypeExamples.Count();

Console.WriteLine($"Archetypes: {archetypeCount}");
Console.WriteLine($"ArchetypePoints: {pointCount}");
Console.WriteLine($"ArchetypeExamples: {exampleCount}");

if (archetypeCount > 0) {
    var sample = db.Archetypes.Include(a => a.Points).FirstOrDefault();
    if (sample != null) {
        Console.WriteLine($"\nSample: {sample.Name}");
        Console.WriteLine($"Points: {sample.Points.Count}");
        if (sample.Points.Any()) {
            var p = sample.Points.First();
            Console.WriteLine($"First point coords: ({p.X}, {p.Y})");
        }
    }
}
