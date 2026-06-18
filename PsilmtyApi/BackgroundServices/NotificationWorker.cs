using Microsoft.Extensions.Options;
using PsilmtyApi.Interfaces.Services;
using PsilmtyApi.Models.Options;

namespace PsilmtyApi.BackgroundServices;

public sealed class NotificationWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<NotificationOptions> options,
    ILogger<NotificationWorker> logger) : BackgroundService
{
    private readonly TimeSpan _delay = TimeSpan.FromSeconds(Math.Max(5, options.Value.PollingSeconds));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<INotificationService>();
                await service.DispatchPendingAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Notification worker cycle failed.");
            }

            await Task.Delay(_delay, stoppingToken);
        }
    }
}
