/**
 * Periodically removes expired JWT jti rows from the database.
 *
 * Hosted on the API process: only schedules the run and opens a
 * DI scope. The cleanup itself lives in the BLL (IAuthService).
 */

//
//  Imports
//

using Backend.BLL.Interfaces;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Backend.API;

//
//  Class
//

public class TokenCleanupBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<TokenCleanupBackgroundService> logger
) : BackgroundService
{
    //
    //  Properties
    //

    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);

    private readonly IServiceScopeFactory scopeFactory = scopeFactory;
    private readonly ILogger<TokenCleanupBackgroundService> logger = logger;

    //
    //  Methods
    //

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken
    )
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = this.scopeFactory.CreateScope();
                var authService = scope.ServiceProvider
                    .GetRequiredService<IAuthService>();

                int removed = await authService
                    .CleanupExpiredTokensAsync();

                if (removed > 0)
                {
                    this.logger.LogInformation(
                        "Removed {Count} expired user token(s).",
                        removed
                    );
                }
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    ex,
                    "Failed to clean up expired user tokens."
                );
            }

            try
            {
                await Task.Delay(Interval, stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}
