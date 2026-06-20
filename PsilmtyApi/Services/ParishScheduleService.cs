using Dapper;
using PsilmtyApi.Interfaces.Data;
using PsilmtyApi.Interfaces.Services;
using PsilmtyApi.Models.Requests;
using PsilmtyApi.Models.Responses;

namespace PsilmtyApi.Services;

public sealed class ParishScheduleService(IDatabaseConnectionFactory connectionFactory) : IParishScheduleService
{
    private static readonly string[] DayNames =
        ["Domingo", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado"];

    public async Task<IReadOnlyList<ParishScheduleDayResponse>> GetAsync(uint parishId)
    {
        await using var connection = connectionFactory.CreateConnection();
        var rows = (await connection.QueryAsync<ScheduleRow>("""
            SELECT id, day_of_week DayOfWeek, open_time OpenTime, close_time CloseTime,
                   is_closed IsClosed, sort_order SortOrder, notes Notes
            FROM parish_schedules
            WHERE parish_id=@ParishId AND status=1
            ORDER BY day_of_week,sort_order
            """, new { ParishId = parishId })).AsList();

        return Enumerable.Range(0, 7).Select(day =>
        {
            var dayRows = rows.Where(row => row.DayOfWeek == day).ToArray();
            var closed = dayRows.FirstOrDefault(row => row.IsClosed);
            return new ParishScheduleDayResponse
            {
                DayOfWeek = (byte)day,
                DayName = DayNames[day],
                IsClosed = closed is not null,
                Notes = closed?.Notes ?? dayRows.FirstOrDefault()?.Notes,
                Blocks = dayRows
                    .Where(row => !row.IsClosed && row.OpenTime.HasValue && row.CloseTime.HasValue)
                    .Select(row => new ParishScheduleBlockResponse
                    {
                        Id = row.Id,
                        OpenTime = TimeOnly.FromTimeSpan(row.OpenTime!.Value),
                        CloseTime = TimeOnly.FromTimeSpan(row.CloseTime!.Value)
                    }).ToArray()
            };
        }).ToArray();
    }

    public async Task<IReadOnlyList<ParishScheduleDayResponse>> ReplaceAsync(
        uint parishId,
        uint userId,
        ParishScheduleRequest request,
        CancellationToken cancellationToken)
    {
        Validate(request);
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await connection.ExecuteAsync(
            "UPDATE parish_schedules SET status=0,updated_at=UTC_TIMESTAMP(),updated_by=@UserId WHERE parish_id=@ParishId",
            new { ParishId = parishId, UserId = userId }, transaction);

        foreach (var day in request.Days)
        {
            if (day.IsClosed)
            {
                await connection.ExecuteAsync("""
                    INSERT INTO parish_schedules
                        (parish_id,day_of_week,is_closed,sort_order,notes,status,created_at,created_by)
                    VALUES (@ParishId,@DayOfWeek,1,1,@Notes,1,UTC_TIMESTAMP(),@UserId)
                    ON DUPLICATE KEY UPDATE open_time=NULL,close_time=NULL,is_closed=1,notes=@Notes,
                        status=1,updated_at=UTC_TIMESTAMP(),updated_by=@UserId
                    """, new { ParishId = parishId, day.DayOfWeek, day.Notes, UserId = userId }, transaction);
                continue;
            }

            var sortOrder = 1;
            foreach (var block in day.Blocks.OrderBy(item => item.OpenTime))
            {
                await connection.ExecuteAsync("""
                    INSERT INTO parish_schedules
                        (parish_id,day_of_week,open_time,close_time,is_closed,sort_order,notes,status,created_at,created_by)
                    VALUES (@ParishId,@DayOfWeek,@OpenTime,@CloseTime,0,@SortOrder,@Notes,1,UTC_TIMESTAMP(),@UserId)
                    ON DUPLICATE KEY UPDATE open_time=@OpenTime,close_time=@CloseTime,is_closed=0,notes=@Notes,
                        status=1,updated_at=UTC_TIMESTAMP(),updated_by=@UserId
                    """, new
                {
                    ParishId = parishId,
                    day.DayOfWeek,
                    OpenTime = block.OpenTime.ToTimeSpan(),
                    CloseTime = block.CloseTime.ToTimeSpan(),
                    SortOrder = sortOrder++,
                    day.Notes,
                    UserId = userId
                }, transaction);
            }
        }

        await transaction.CommitAsync(cancellationToken);
        return await GetAsync(parishId);
    }

    private static void Validate(ParishScheduleRequest request)
    {
        if (request.Days.GroupBy(day => day.DayOfWeek).Any(group => group.Count() > 1))
            throw new InvalidOperationException("Each day may only appear once.");

        foreach (var day in request.Days)
        {
            if (day.DayOfWeek > 6) throw new InvalidOperationException("Day of week must be between 0 and 6.");
            if (day.IsClosed && day.Blocks.Count > 0)
                throw new InvalidOperationException($"{DayNames[day.DayOfWeek]} cannot be closed and contain opening blocks.");
            if (!day.IsClosed && day.Blocks.Count == 0)
                throw new InvalidOperationException($"{DayNames[day.DayOfWeek]} requires at least one opening block.");
            if (day.Blocks.Any(block => block.OpenTime >= block.CloseTime))
                throw new InvalidOperationException($"{DayNames[day.DayOfWeek]} contains an invalid time range.");
        }
    }

    private sealed class ScheduleRow
    {
        public uint Id { get; set; }
        public byte DayOfWeek { get; set; }
        public TimeSpan? OpenTime { get; set; }
        public TimeSpan? CloseTime { get; set; }
        public bool IsClosed { get; set; }
        public byte SortOrder { get; set; }
        public string? Notes { get; set; }
    }
}
