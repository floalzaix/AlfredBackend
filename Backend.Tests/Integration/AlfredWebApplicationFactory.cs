/**
 * Starts Backend.API in-process for integration tests.
 *
 * Swaps PostgreSQL for a temp SQLite file and injects a
 * fixed JWT key so tests do not need Docker or user-secrets.
 */

//
//  Imports
//

namespace Backend.Tests.Integration;

using Backend.DAL;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

//
//  Class
//

public class AlfredWebApplicationFactory
    : WebApplicationFactory<Program>, IAsyncLifetime
{
    //
    //  Properties
    //

    // 32 bytes Base64 (>= 44 chars) — matches JwtConfig rules.
    public const string TestJwtKey =
        "dGVzdC1qd3Qta2V5LWZvci1hbGZyZWQtYmFja2VuZCEhISE=";

    private readonly string dbPath = Path.Combine(
        Path.GetTempPath(),
        $"alfred-tests-{Guid.NewGuid():N}.db"
    );

    private readonly SqliteConnection connection;

    //
    //  Constructor
    //

    public AlfredWebApplicationFactory()
    {
        this.connection = new SqliteConnection(
            $"Data Source={this.dbPath}"
        );
        this.connection.Open();
    }

    //
    //  Methods
    //

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        // UseSetting is applied early enough for both Program.cs
        // TokenValidationParameters and IOptions<JwtConfig>.
        builder.UseSetting("JwtConfig:Key", TestJwtKey);
        builder.UseSetting("JwtConfig:Issuer", "alfred_backend");
        builder.UseSetting("JwtConfig:Audience", "alfred_backend");
        builder.UseSetting("JwtConfig:ExpirationTime", "60");
        builder.UseSetting(
            "DALConfig:ConnectionString",
            "Host=localhost;Database=alfred_tests;Username=test;Password=test"
        );
        builder.UseSetting("AppConfig:UseHttpsRedirection", "false");
        builder.UseSetting("Cors:AllowedOrigins:0", "http://localhost:8080");

        builder.ConfigureTestServices(services =>
        {
            // EF Core 8+ keeps the provider on this interface.
            // Removing only DbContextOptions would leave Npgsql.
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(this.connection);
            });
        });
    }

    async Task IAsyncLifetime.InitializeAsync()
    {
        using IServiceScope scope = this.Services.CreateScope();
        AppDbContext db = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await this.DisposeAsync();
        await this.connection.DisposeAsync();

        try
        {
            if (File.Exists(this.dbPath))
            {
                File.Delete(this.dbPath);
            }
        }
        catch (IOException)
        {
            // Temp file cleanup is best-effort after SQLite unlock.
        }
    }
}
