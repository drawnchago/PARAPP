namespace PsilmtyApi.Models.Requests;

public sealed class ParishScheduleRequest
{
    public IReadOnlyList<ParishScheduleDayRequest> Days { get; set; } = [];
}

public sealed class ParishScheduleDayRequest
{
    public byte DayOfWeek { get; set; }
    public bool IsClosed { get; set; }
    public string? Notes { get; set; }
    public IReadOnlyList<ParishScheduleBlockRequest> Blocks { get; set; } = [];
}

public sealed class ParishScheduleBlockRequest
{
    public TimeOnly OpenTime { get; set; }
    public TimeOnly CloseTime { get; set; }
}
