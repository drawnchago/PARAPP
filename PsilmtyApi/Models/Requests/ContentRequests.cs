namespace PsilmtyApi.Models.Requests;

public sealed class NewsRequest
{
    public string Title { get; set; } = "";
    public string? Summary { get; set; }
    public string Content { get; set; } = "";
    public string? ImageUrl { get; set; }
    public uint? CategoryId { get; set; }
    public bool IsPublished { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime? PublishedAt { get; set; }
    public bool SendNotification { get; set; }
    public int ReminderMinutes { get; set; }
}

public sealed class CalendarRequest
{
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public string Color { get; set; } = "#CC0000";
    public DateTime StartDatetime { get; set; }
    public DateTime EndDatetime { get; set; }
    public bool AllDay { get; set; }
    public string Type { get; set; } = "event";
    public string? Location { get; set; }
    public int ReminderMinutes { get; set; } = 60;
    public bool SendNotification { get; set; }
}

public sealed class NotificationRequest
{
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public string? ImageUrl { get; set; }
    public string Type { get; set; } = "general";
    public string Audience { get; set; } = "all";
    public uint? AudienceRoleId { get; set; }
    public uint? AudienceUserId { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public string? ReferenceType { get; set; }
    public uint? ReferenceId { get; set; }
}

public sealed class DeviceRegistrationRequest
{
    public string PushToken { get; set; } = "";
    public string Platform { get; set; } = "android";
    public string? DeviceModel { get; set; }
    public string? OsVersion { get; set; }
    public string? AppVersion { get; set; }
}
