namespace PsilmtyApi.Interfaces.Services;

public interface IPushNotificationSender
{
    Task<int> SendAsync(string title, string body, string? imageUrl, IReadOnlyList<string> tokens, CancellationToken cancellationToken);
}
