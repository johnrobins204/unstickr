using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Moq.Protected;
using StoryFort.Services;
using StoryFort.Models;
using Xunit;

namespace StoryFort.Tests.Unit;

/// <summary>
/// Spec-driven tests for CohereTutorService
/// Spec: /specs/cohere-tutor-service.md
/// Date: February 2, 2026
/// </summary>
public class CohereTutorService_Specs
{
    private HttpClient MakeClient(HttpResponseMessage responseMessage, Action<HttpRequestMessage>? onRequest = null)
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock.Protected()
           .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
           .ReturnsAsync((HttpRequestMessage req, CancellationToken ct) =>
           {
               onRequest?.Invoke(req);
               return responseMessage;
           });

        var client = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://api.cohere.fake/")
        };
        return client;
    }

    [Fact]
    public async Task GetSocraticPromptAsync_WithChatResponse_ReturnsText()
    {
        var genObj = new StoryFort.Services.CohereGenerateResponse { Generations = new System.Collections.Generic.List<StoryFort.Services.CohereGeneration> { new() { Text = "Hello from LLM" } } };
        var json = System.Text.Json.JsonSerializer.Serialize(genObj);
        var msg = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        HttpRequestMessage captured = null!;
        var client = MakeClient(msg, req => captured = req);

        var factoryMock = new Moq.Mock<System.Net.Http.IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("LLM")).Returns(client);
        var protectorMock = new Moq.Mock<IApiKeyProtector>();
        protectorMock.Setup(p => p.Unprotect(It.IsAny<string>())).Returns("real-key");
        var svc = new CohereTutorService(factoryMock.Object, protectorMock.Object);

        var result = await svc.GetSocraticPromptAsync("hi", new Account { ProtectedCohereApiKey = "p" }, false);

        result.Should().NotBeNull();
        result.Should().Contain("Hello from LLM");
        captured.Headers.Should().ContainKey("Authorization");
    }

    [Fact]
    public async Task GetSocraticPromptAsync_WithReasoningResponse_ReturnsThoughtsAndOutput()
    {
        var chatObj = new StoryFort.Services.CohereChatResponse { Message = new StoryFort.Services.CohereMessage { Content = new System.Collections.Generic.List<StoryFort.Services.CohereContentItem> { new() { Type = "text", Text = "Final answer" } } } };
        var json = System.Text.Json.JsonSerializer.Serialize(chatObj);
        var msg = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        var client = MakeClient(msg);
        var factoryMock = new Moq.Mock<System.Net.Http.IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("LLM")).Returns(client);
        var protectorMock = new Moq.Mock<IApiKeyProtector>();
        protectorMock.Setup(p => p.Unprotect(It.IsAny<string>())).Returns("real-key");
        var svc = new CohereTutorService(factoryMock.Object, protectorMock.Object);

        var result = await svc.GetSocraticPromptAsync("prompt", new Account { ProtectedCohereApiKey = "p" }, true);

        result.Should().Contain("Final answer");
    }

    [Fact]
    public async Task GetSocraticPromptAsync_Unauthorized_ReturnsErrorString()
    {
        var msg = new HttpResponseMessage(HttpStatusCode.Unauthorized);
        var client = MakeClient(msg);
        var factoryMock = new Moq.Mock<System.Net.Http.IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("LLM")).Returns(client);
        var protectorMock = new Moq.Mock<IApiKeyProtector>();
        protectorMock.Setup(p => p.Unprotect(It.IsAny<string>())).Returns("real-key");
        var svc = new CohereTutorService(factoryMock.Object, protectorMock.Object);

        var result = await svc.GetSocraticPromptAsync("p", new Account { ProtectedCohereApiKey = "p" }, false);
        result.Should().Contain("AI service error");
    }

    [Fact]
    public async Task GetSocraticPromptAsync_RateLimit_RetriesAndSucceeds()
    {
        var first = new HttpResponseMessage((HttpStatusCode)429);
        first.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromSeconds(0));
        var secondObj = new StoryFort.Services.CohereGenerateResponse { Generations = new System.Collections.Generic.List<StoryFort.Services.CohereGeneration> { new() { Text = "ok" } } };
        var second = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(secondObj), Encoding.UTF8, "application/json")
        };

        var call = 0;
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock.Protected()
           .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
           .ReturnsAsync(() => call++ == 0 ? first : second);
        var client = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("https://api.cohere.fake/") };

        var factoryMock = new Moq.Mock<System.Net.Http.IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("LLM")).Returns(client);
        var protectorMock = new Moq.Mock<IApiKeyProtector>();
        protectorMock.Setup(p => p.Unprotect(It.IsAny<string>())).Returns("real-key");
        var svc = new CohereTutorService(factoryMock.Object, protectorMock.Object);

        var result = await svc.GetSocraticPromptAsync("p", new Account { ProtectedCohereApiKey = "p" }, false);
        result.Should().Contain("ok");
    }

    [Fact]
    public async Task GetSocraticPromptAsync_MalformedJson_ReturnsErrorString()
    {
        var msg = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{not-a-json}", Encoding.UTF8, "application/json")
        };
        var client = MakeClient(msg);
        var factoryMock = new Moq.Mock<System.Net.Http.IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("LLM")).Returns(client);
        var protectorMock = new Moq.Mock<IApiKeyProtector>();
        protectorMock.Setup(p => p.Unprotect(It.IsAny<string>())).Returns("real-key");
        var svc = new CohereTutorService(factoryMock.Object, protectorMock.Object);

        var result = await svc.GetSocraticPromptAsync("p", new Account { ProtectedCohereApiKey = "p" }, false);
        result.Should().Contain("AI service error");
    }

    [Fact]
    public async Task GetSocraticPromptAsync_Timeout_ReturnsErrorString()
    {
          var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
          handlerMock.Protected()
              .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
              .ThrowsAsync(new TaskCanceledException());
        var client = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("https://api.cohere.fake/") };

        var factoryMock = new Moq.Mock<System.Net.Http.IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("LLM")).Returns(client);
        var protectorMock = new Moq.Mock<IApiKeyProtector>();
        protectorMock.Setup(p => p.Unprotect(It.IsAny<string>())).Returns("real-key");
        var svc = new CohereTutorService(factoryMock.Object, protectorMock.Object);

        var result = await svc.GetSocraticPromptAsync("p", new Account { ProtectedCohereApiKey = "p" }, false);
        result.Should().Contain("AI service error");
    }
}
