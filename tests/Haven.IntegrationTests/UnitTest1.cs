using Microsoft.AspNetCore.Mvc.Testing;

namespace Haven.IntegrationTests;

public sealed class ApplicationSmokeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApplicationSmokeTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HomePage_ShouldReturnSuccess()
    {
        using var response = await _client.GetAsync("/");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task OpenApiDocument_ShouldReturnJsonInDevelopment()
    {
        using var response = await _client.GetAsync("/openapi/v1.json");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("openapi", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ScalarReference_ShouldBeAvailableInDevelopment()
    {
        using var response = await _client.GetAsync("/scalar/v1");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("text/html", response.Content.Headers.ContentType?.MediaType);
    }
}
