using DataModel.Repository;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using WebApi.IntegrationTests.Fixtures;
using WebApi.IntegrationTests.Helpers;
using Xunit.Abstractions;

namespace WebApi.IntegrationTests;

public class SomeIntegrationTests : IClassFixture<IntegrationTestsWebApplicationFactory<Program>>, IClassFixture<RabbitMqFixture>
{
    private readonly IntegrationTestsWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;
    private readonly RabbitMqFixture _rabbitMqFixture;

    public SomeIntegrationTests(IntegrationTestsWebApplicationFactory<Program> factory, RabbitMqFixture rabbitMqFixture, ITestOutputHelper output)
    {
        _factory = factory;
        _rabbitMqFixture = rabbitMqFixture;
        _output = output;

        _factory.RabbitMqConnectionString = _rabbitMqFixture.GetConnectionString();
        Environment.SetEnvironmentVariable("Arg", "Repl1");  // Ensure this is set before creating the client
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Theory]
    [InlineData("/api/Project")]
    public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
    {
        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());
    }

    [Theory]
    [InlineData("/api/Project")]
    public async Task Get_EndpointsReturnData(string url)
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<AbsanteeContext>();

            Utilities.ReinitializeDbForTests(db);
        }

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        var responseBody = await response.Content.ReadAsStringAsync();
        _output.WriteLine(responseBody);
        Assert.NotNull(responseBody);

        var jsonArray = JArray.Parse(responseBody);

        Assert.True(jsonArray.Type == JTokenType.Array, "Response body is not a JSON array");
        Assert.Equal(3, jsonArray.Count);
    }

    [Theory]
    [InlineData("/api/Project/1", "Project 1")]
    // [InlineData("/api/Project/2", "Project 2")]
    public async Task Get_EndpointsIdReturnData(string url, string expectedProject)
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<AbsanteeContext>();

            Utilities.ReinitializeDbForTests(db);
        }

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        var responseBody = await response.Content.ReadAsStringAsync();
        response.EnsureSuccessStatusCode(); // Status Code 200-299
        _output.WriteLine(responseBody);

        Assert.NotNull(responseBody);

        var jsonResponse = JObject.Parse(responseBody);
        _output.WriteLine(jsonResponse.ToString());

        Assert.True(jsonResponse.ContainsKey("name"), "Response JSON does not contain 'name' key");
        var projectName = jsonResponse["name"].ToString();
        Assert.Equal(expectedProject, projectName);
    }
}
