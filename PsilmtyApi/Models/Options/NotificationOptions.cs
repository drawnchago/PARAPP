namespace PsilmtyApi.Models.Options;

public sealed class NotificationOptions
{
    public const string SectionName = "Notifications";
    public int PollingSeconds { get; set; } = 30;
    public int BatchSize { get; set; } = 100;
}
