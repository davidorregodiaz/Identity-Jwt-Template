using System.Text.Json;
using Infraestructure.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Utilities;

namespace Infraestructure.Services.BackgroundServices;

public class RefreshTokenCleanup : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RefreshTokenCleanup> _logger;

    public RefreshTokenCleanup(IServiceProvider serviceProvider, ILogger<RefreshTokenCleanup> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
        
        var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await CleanupRefreshTokens(context);

    }

    private async Task CleanupRefreshTokens(AppDbContext context)
    {
        var allTokens = context.UserTokens.ToList();
        
        var expiredTokens = new List<IdentityUserToken<string>>();

        foreach (var token in allTokens)
        {
            try
            {
                var tokenData = JsonSerializer.Deserialize<TokenData>(token.Value!);

                if (tokenData != null || tokenData.IsExpired)
                {
                    expiredTokens.Add(token);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        if (expiredTokens.Any())
        {
            context.UserTokens.RemoveRange(expiredTokens);
            await context.SaveChangesAsync();
            _logger.LogInformation($"Refresh token cleanup complete, number of tokens removed: {expiredTokens.Count}");
        }
    }
}