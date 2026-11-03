using Haven.Infrastructure.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Haven.IntegrationTests;

public sealed class ApplicationSmokeTests : IClassFixture<SmokeTestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ApplicationSmokeTests(SmokeTestWebApplicationFactory factory)
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

public sealed class SmokeTestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<
                IDbContextOptionsConfiguration<ApplicationDbContext>>();
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<ApplicationDbContext>();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("Haven.ApplicationSmokeTests"));
        });
    }
}
