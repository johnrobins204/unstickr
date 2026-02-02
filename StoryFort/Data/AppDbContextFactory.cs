using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace StoryFort.Data
{
    // Design-time factory for EF tools to create the AppDbContext without the full app DI.
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            // Use the same file-based SQLite DB as the application
            optionsBuilder.UseSqlite("Data Source=StoryFort.db");
            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
