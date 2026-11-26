/**
 * Integration tests for the Auth HTTP routes.
 *
 * Goes through the real stack (route → controller → service
 * → SQLite) to check tokens, validation, ProblemDetails and
 * logout revocation.
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
public class AuthIntegrationTests {
    //
    //  Properties
    //

    private readonly HttpClient client;

    private static readonly JsonSerializerOptions JsonOptions = new() {
        PropertyNameCaseInsensitive = true
    };

    //
    //  Constructor
    //

    public AuthIntegrationTests(AlfredWebApplicationFactory factory) {
        this.client = factory.CreateClient();
    }

    //
    //  Methods
    //

    [Fact]
    public async Task Register_ShouldReturnBearerToken() {
        string email = this.UniqueEmail();

        HttpResponseMessage response = await this.client.PostAsJsonAsync(
            "/api/auth/register",
            new {
                username = "alice",
                email,
                password = "Password1!"
            }
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        TokenResponse? body = await response.Content
            .ReadFromJsonAsync<TokenResponse>(JsonOptions);

        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body!.AccessToken));
        Assert.Equal("Bearer", body.TokenType);
    }

    [Fact]
    public async Task Register_WhenEmailAlreadyUsed_ShouldReturn409() {
        string email = this.UniqueEmail();
        await this.RegisterAsync(email, "Password1!");

        HttpResponseMessage response = await this.client.PostAsJsonAsync(
            "/api/auth/register",
            new {
                username = "other",
                email,
                password = "Password1!"
            }
        );

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        using JsonDocument doc = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync()
        );
        Assert.Equal(
            "Email déjà utilisé.",
            doc.RootElement.GetProperty("detail").GetString()
        );
    }

    [Fact]
    public async Task Register_WithInvalidPayload_ShouldReturn400() {
        HttpResponseMessage response = await this.client.PostAsJsonAsync(
            "/api/auth/register",
            new {
                username = "ab",
                email = "not-an-email",
                password = "short"
            }
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnBearerToken() {
        string email = this.UniqueEmail();
        await this.RegisterAsync(email, "Password1!");

        HttpResponseMessage response = await this.client.PostAsJsonAsync(
            "/api/auth/login",
            new {
                email,
                password = "Password1!"
            }
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        TokenResponse? body = await response.Content
            .ReadFromJsonAsync<TokenResponse>(JsonOptions);

        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body!.AccessToken));
        Assert.Equal("Bearer", body.TokenType);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ShouldReturn401() {
        string email = this.UniqueEmail();
        await this.RegisterAsync(email, "Password1!");

        HttpResponseMessage response = await this.client.PostAsJsonAsync(
            "/api/auth/login",
            new {
                email,
                password = "WrongPass1!"
            }
        );

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        using JsonDocument doc = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync()
        );
        Assert.Equal(
            "Mot de passe invalide.",
            doc.RootElement.GetProperty("detail").GetString()
        );
    }

    [Fact]
    public async Task Me_WithValidToken_ShouldReturnCurrentUser() {
        string email = this.UniqueEmail();
        string token = await this.RegisterAsync(email, "Password1!");

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/api/auth/me"
        );
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await this.client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using JsonDocument doc = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync()
        );
        Assert.Equal(
            email,
            doc.RootElement.GetProperty("email").GetString()
        );
        Assert.Equal(
            "alice",
            doc.RootElement.GetProperty("username").GetString()
        );
        Assert.True(
            doc.RootElement.TryGetProperty("id", out JsonElement id)
        );
        Assert.NotEqual(Guid.Empty, id.GetGuid());
    }

    [Fact]
    public async Task Me_WithoutToken_ShouldReturn401() {
        HttpResponseMessage response = await this.client.GetAsync(
            "/api/auth/me"
        );

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Logout_ShouldRevokeTokenForProtectedRoutes() {
        string email = this.UniqueEmail();
        string token = await this.RegisterAsync(email, "Password1!");

        using var logoutRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/auth/logout"
        );
        logoutRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage logoutResponse =
            await this.client.SendAsync(logoutRequest);
        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);

        using var meRequest = new HttpRequestMessage(
            HttpMethod.Get,
            "/api/auth/me"
        );
        meRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage meResponse =
            await this.client.SendAsync(meRequest);

        // JWT signature is still valid, but ActiveJtiRequirement
        // rejects the revoked jti → 403 Forbidden.
        Assert.Equal(HttpStatusCode.Forbidden, meResponse.StatusCode);
    }

    private async Task<string> RegisterAsync(
        string email,
        string password
    ) {
        HttpResponseMessage response = await this.client.PostAsJsonAsync(
            "/api/auth/register",
            new {
                username = "alice",
                email,
                password
            }
        );

        response.EnsureSuccessStatusCode();

        TokenResponse? body = await response.Content
            .ReadFromJsonAsync<TokenResponse>(JsonOptions);

        Assert.NotNull(body);
        return body!.AccessToken;
    }

    private string UniqueEmail() {
        return $"user-{Guid.NewGuid():N}@example.com";
    }

    //
    //  Nested types
    //

    private sealed class TokenResponse {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = string.Empty;
    }
}
