using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using StoryFort.Services;

namespace StoryFort.Tests.Integration;

public class ApiPinsTests : IClassFixture<WebApplicationFactory<Program>>
{
	private readonly WebApplicationFactory<Program> _factory;

	public ApiPinsTests(WebApplicationFactory<Program> factory)
	{
		_factory = factory.WithWebHostBuilder(builder =>
		{
			builder.ConfigureServices(services =>
			{
				// Replace ICohereTutorService with a fake implementation for tests
				services.AddSingleton<ICohereTutorService, FakeCohereTutorService>();
			});
		});
	}

	[Fact]
	public async Task RootEndpoint_ReturnsSuccess_WithMockedLLM()
	{
		var client = _factory.CreateClient();
		var resp = await client.GetAsync("/");
		resp.EnsureSuccessStatusCode();
	}

	private class FakeCohereTutorService : ICohereTutorService
	{
		public Task<string> GetSocraticPromptAsync(string prompt, Models.Account account, bool useReasoningModel)
		{
			return Task.FromResult("MOCK_RESPONSE");
		}
	}
}