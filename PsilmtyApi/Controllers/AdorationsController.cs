using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PsilmtyApi.Helpers;
using PsilmtyApi.Interfaces.Repositories;
using PsilmtyApi.Models.Requests;

namespace PsilmtyApi.Controllers;

[Authorize]
[ApiController]
[Route("api/adoraciones")]
public sealed class AdorationsController(IDatabaseRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await repository.QueryAsync<object>("""
        SELECT e.id Id,e.title Titulo,e.start_datetime Fecha,
               TIMESTAMPDIFF(MINUTE,e.start_datetime,e.end_datetime) Duracion,
               COALESCE(c.name,'mixta') Tipo,IF(e.is_active=1,'active','inactive') Estado,
               e.description Notas,CONCAT_WS(' ',u.first_name,u.last_name) NombreCreador,
               e.created_at CreatedAt
        FROM events e
        LEFT JOIN event_categories c ON c.id=e.category_id
        LEFT JOIN users u ON u.id=e.created_by_user
        WHERE e.parish_id=@ParishId AND e.is_mass=1 AND e.is_active=1
        ORDER BY e.start_datetime DESC
        """, new { ParishId = User.GetParishId() }));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(uint id) =>
        Ok(await repository.QuerySingleAsync<object>("""
            SELECT e.id Id,e.title Titulo,e.start_datetime Fecha,
                   TIMESTAMPDIFF(MINUTE,e.start_datetime,e.end_datetime) Duracion,
                   COALESCE(c.name,'mixta') Tipo,IF(e.is_active=1,'active','inactive') Estado,
                   e.description Notas,CONCAT_WS(' ',u.first_name,u.last_name) NombreCreador,
                   e.created_at CreatedAt
            FROM events e
            LEFT JOIN event_categories c ON c.id=e.category_id
            LEFT JOIN users u ON u.id=e.created_by_user
            WHERE e.id=@Id AND e.parish_id=@ParishId AND e.is_mass=1
            """, new { Id = id, ParishId = User.GetParishId() }));

    [HttpPost]
    public async Task<IActionResult> Create(AdorationRequest request)
    {
        var endDate = request.Date.AddMinutes(request.DurationMinutes ?? 60);
        var id = await repository.ExecuteScalarAsync<uint>("""
            INSERT INTO events
                (parish_id,created_by_user,title,description,start_datetime,end_datetime,
                 is_mass,is_public,is_active,created_at,created_by)
            VALUES
                (@ParishId,@UserId,@Title,@Notes,@StartDate,@EndDate,1,1,1,UTC_TIMESTAMP(),@UserId);
            SELECT LAST_INSERT_ID();
            """, new
        {
            ParishId = User.GetParishId(), UserId = User.GetUserId(),
            request.Title, request.Notes, StartDate = request.Date, EndDate = endDate
        });
        return await GetById(id);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(uint id)
    {
        await repository.ExecuteAsync(
            "UPDATE events SET is_active=0,updated_at=UTC_TIMESTAMP(),updated_by=@UserId WHERE id=@Id AND parish_id=@ParishId AND is_mass=1",
            new { Id = id, UserId = User.GetUserId(), ParishId = User.GetParishId() });
        return NoContent();
    }
}
