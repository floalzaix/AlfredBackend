/**
 * Integration tests for the Example HTTP routes.
 *
 * Checks that the protected demo route needs a valid Bearer
 * token and returns the expected payload.
 */

//
//  Imports
//

namespace Backend.Tests.Integration;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

//
//  Class
//

[Collection("Integration")]
public class ExampleIntegrationTests
{
    //
    //  Properties
    //

    private readonly HttpClient client;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    //
    //  Constructor
    //

    public ExampleIntegrationTests(AlfredWebApplicationFactory factory)
    {
        this.client = factory.CreateClient();
    }

    //
    //  Methods
    //

    [Fact]
    public async Task HelloWorld_WithoutToken_ShouldReturn401()
    {
        HttpResponseMessage response = await this.client.GetAsync(
            "/api/example/hello_world?Name=Bob"
        );

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task HelloWorld_WithValidToken_ShouldReturnNamedUser()
    {
        string token = await this.RegisterAsync();

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/api/example/hello_world?Name=Bob"
        );
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await this.client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using JsonDocument doc = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync()
        );
        Assert.Equal(
            "Bob",
            doc.RootElement.GetProperty("name").GetString()
        );
        Assert.True(
            doc.RootElement.TryGetProperty("id", out JsonElement id)
        );
        Assert.NotEqual(Guid.Empty, id.GetGuid());
    }

    [Fact]
    public async Task HelloWorld_WithInvalidName_ShouldReturn400()
    {
        string token = await this.RegisterAsync();

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/api/example/hello_world?Name="
        );
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await this.client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<string> RegisterAsync()
    {
        HttpResponseMessage response = await this.client.PostAsJsonAsync(
            "/api/auth/register",
            new
            {
                username = "alice",
                email = $"example-{Guid.NewGuid():N}@example.com",
                password = "Password1!"
            }
        );

        response.EnsureSuccessStatusCode();

        TokenResponse? body = await response.Content
            .ReadFromJsonAsync<TokenResponse>(JsonOptions);

        Assert.NotNull(body);
        return body!.AccessToken;
    }

    //
    //  Nested types
    //

    private sealed class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = string.Empty;
    }
}
