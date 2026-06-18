namespace PsilmtyApi.Models.Responses;

public sealed class ParishScheduleDayResponse
{
    public byte DayOfWeek { get; set; }
    public string DayName { get; set; } = "";
    public bool IsClosed { get; set; }
    public string? Notes { get; set; }
    public IReadOnlyList<ParishScheduleBlockResponse> Blocks { get; set; } = [];
}

public sealed class ParishScheduleBlockResponse
{
    public uint Id { get; set; }
    public TimeOnly OpenTime { get; set; }
    public TimeOnly CloseTime { get; set; }
}
