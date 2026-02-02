using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StoryFort.Data;
using StoryFort.Models;
using Xunit;

namespace StoryFort.Tests.Unit;

public class NotebookEntityTests
{
	[Fact]
	public async Task NotebookEntity_Metadata_HandlesInvalidJson()
	{
		var services = new ServiceCollection();
		var dbName = System.Guid.NewGuid().ToString();
		services.AddDbContext<AppDbContext>(opts => opts.UseInMemoryDatabase(dbName));
		var provider = services.BuildServiceProvider();

		using var scope = provider.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
		var nb = new Notebook { Name = "N" };
		db.Notebooks.Add(nb);
		await db.SaveChangesAsync();

		// Create an entity with invalid metadata JSON
		var ent = new NotebookEntity { NotebookId = nb.Id, Name = "E", Metadata = "{ not valid json }" };
		db.NotebookEntities.Add(ent);
		await db.SaveChangesAsync();

		var fetched = await db.NotebookEntities.FindAsync(ent.Id);
		fetched.Should().NotBeNull();
		// Accessing Metadata should not throw; model stores as string so we assert it's stored as-is
		fetched!.Metadata.Should().Be("{ not valid json }");
	}
}