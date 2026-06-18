using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PsilmtyApi.Helpers;
using PsilmtyApi.Interfaces.Repositories;

namespace PsilmtyApi.Controllers;

[Authorize]
[ApiController]
[Route("api/eventos")]
public sealed class EventsController(IDatabaseRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await repository.QueryAsync<object>("""
        SELECT e.id Id,e.title Titulo,e.description Descripcion,e.start_datetime FechaInicio,
               e.end_datetime FechaFin,e.location Ubicacion,c.name Tipo,
               IF(e.is_active=1,'active','inactive') Estado,e.max_capacity Capacidad,
               CONCAT_WS(' ',u.first_name,u.last_name) NombreCreador,e.created_at CreatedAt
        FROM events e
        LEFT JOIN event_categories c ON c.id=e.category_id
        LEFT JOIN users u ON u.id=e.created_by_user
        WHERE e.parish_id=@ParishId AND e.is_active=1 ORDER BY e.start_datetime
        """, new { ParishId = User.GetParishId() }));

    [HttpPost]
    public async Task<IActionResult> Create(Dictionary<string, object?> request)
    {
        var id = await repository.ExecuteScalarAsync<uint>("""
            INSERT INTO events
                (parish_id,created_by_user,title,description,location,start_datetime,end_datetime,
                 max_capacity,is_public,is_active,created_at,created_by)
            VALUES
                (@ParishId,@UserId,@Title,@Description,@Location,@StartDate,@EndDate,
                 @Capacity,1,1,UTC_TIMESTAMP(),@UserId);
            SELECT LAST_INSERT_ID();
            """, new
        {
            ParishId = User.GetParishId(), UserId = User.GetUserId(),
            Title = request.GetValueOrDefault("titulo"), Description = request.GetValueOrDefault("descripcion"),
            Location = request.GetValueOrDefault("ubicacion"), StartDate = request.GetValueOrDefault("fechaInicio"),
            EndDate = request.GetValueOrDefault("fechaFin"), Capacity = request.GetValueOrDefault("capacidad")
        });
        return Ok(new { id });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(uint id)
    {
        await repository.ExecuteAsync(
            "UPDATE events SET is_active=0,updated_at=UTC_TIMESTAMP(),updated_by=@UserId WHERE id=@Id AND parish_id=@ParishId",
            new { Id = id, UserId = User.GetUserId(), ParishId = User.GetParishId() });
        return NoContent();
    }
}
