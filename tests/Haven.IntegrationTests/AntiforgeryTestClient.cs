using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Haven.IntegrationTests;

internal static class AntiforgeryTestClient
{
    public static HttpClient Create(
        WebApplicationFactory<Program> factory,
        WebApplicationFactoryClientOptions? options = null)
    {
        var inner = factory.CreateClient(options ?? new WebApplicationFactoryClientOptions
        {
            HandleCookies = true,
            AllowAutoRedirect = false
        });
        var handler = new AntiforgeryHandler(inner);
        var client = new HttpClient(handler, disposeHandler: true)
        {
            BaseAddress = inner.BaseAddress
        };
        return client;
    }

    private sealed class AntiforgeryHandler(HttpClient inner) : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            using var forwarded = await CloneRequestAsync(request, cancellationToken);
            if (request.Method != HttpMethod.Get &&
                request.Method != HttpMethod.Head &&
                request.Method != HttpMethod.Options)
            {
                using var tokenResponse = await inner.GetAsync(
                    new Uri(inner.BaseAddress!, "api/auth/antiforgery"),
                    cancellationToken);
                tokenResponse.EnsureSuccessStatusCode();
                var token = await tokenResponse.Content
                    .ReadFromJsonAsync<TokenResponse>(
                        cancellationToken: cancellationToken);
                if (string.IsNullOrWhiteSpace(token?.RequestToken))
                {
                    throw new InvalidOperationException(
                        "The test server did not issue an antiforgery token.");
                }

                forwarded.Headers.TryAddWithoutValidation(
                    "RequestVerificationToken",
                    token.RequestToken);
            }

            return await inner.SendAsync(forwarded, cancellationToken);
        }

        private static async Task<HttpRequestMessage> CloneRequestAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri)
            {
                Version = request.Version,
                VersionPolicy = request.VersionPolicy
            };
            foreach (var header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            foreach (var option in request.Options)
            {
                clone.Options.Set(new HttpRequestOptionsKey<object?>(option.Key), option.Value);
            }

            if (request.Content is not null)
            {
                var content = new ByteArrayContent(
                    await request.Content.ReadAsByteArrayAsync(cancellationToken));
                foreach (var header in request.Content.Headers)
                {
                    content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                clone.Content = content;
            }

            return clone;
        }
    }

    private sealed record TokenResponse(string? RequestToken);
}