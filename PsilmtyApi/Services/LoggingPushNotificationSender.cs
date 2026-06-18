using PsilmtyApi.Interfaces.Services;

namespace PsilmtyApi.Services;

public sealed class LoggingPushNotificationSender(ILogger<LoggingPushNotificationSender> logger) : IPushNotificationSender
{
    public Task<int> SendAsync(
        string title,
        string body,
        string? imageUrl,
        IReadOnlyList<string> tokens,
        CancellationToken cancellationToken)
    {
        logger.LogWarning(
            "Push delivery provider is not configured. Notification '{Title}' remains queued for {Count} device(s).",
            title,
            tokens.Count);
        return Task.FromResult(0);
    }
}
