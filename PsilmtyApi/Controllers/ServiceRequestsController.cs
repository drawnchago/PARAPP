using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PsilmtyApi.Helpers;
using PsilmtyApi.Interfaces.Repositories;

namespace PsilmtyApi.Controllers;

[Authorize]
[ApiController]
[Route("api/solicitudes")]
public sealed class ServiceRequestsController(IDatabaseRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await repository.QueryAsync<object>("""
        SELECT r.id Id,r.user_id UserId,CONCAT_WS(' ',u.first_name,u.last_name) UserName,
               r.service_id ServiceId,t.name ServiceName,r.description Description,
               r.desired_date DesiredDate,r.status Status,r.response Response,
               CONCAT_WS(' ',a.first_name,a.last_name) AttendedName,r.created_at CreatedAt
        FROM service_requests r
        JOIN users u ON u.id=r.user_id
        JOIN service_types t ON t.id=r.service_id
        LEFT JOIN users a ON a.id=r.attended_by
        WHERE r.parish_id=@ParishId AND r.is_active=1 ORDER BY r.created_at DESC
        """, new { ParishId = User.GetParishId() }));

    [HttpGet("tipos")]
    public async Task<IActionResult> GetTypes() =>
        Ok(await repository.QueryAsync<object>(
            "SELECT id Id,name Name,description Description FROM service_types WHERE status=1 ORDER BY name"));

    [HttpPost]
    public async Task<IActionResult> Create(Dictionary<string, object?> request)
    {
        var id = await repository.ExecuteScalarAsync<uint>("""
            INSERT INTO service_requests
                (parish_id,user_id,service_id,description,desired_date,status,is_active,created_at,created_by)
            VALUES
                (@ParishId,@UserId,@ServiceId,@Description,@DesiredDate,'new',1,UTC_TIMESTAMP(),@UserId);
            SELECT LAST_INSERT_ID();
            """, new
        {
            ParishId = User.GetParishId(), UserId = User.GetUserId(),
            ServiceId = request.GetValueOrDefault("serviceId"),
            Description = request.GetValueOrDefault("description"),
            DesiredDate = request.GetValueOrDefault("desiredDate")
        });
        return Ok(new { id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(uint id, Dictionary<string, object?> request)
    {
        await repository.ExecuteAsync("""
            UPDATE service_requests SET status=COALESCE(@Status,status),response=COALESCE(@Response,response),
                attended_by=IF(@Status IS NULL,attended_by,@UserId),
                attended_at=IF(@Status IS NULL,attended_at,UTC_TIMESTAMP()),
                updated_at=UTC_TIMESTAMP(),updated_by=@UserId
            WHERE id=@Id AND parish_id=@ParishId
            """, new
        {
            Id = id, ParishId = User.GetParishId(), UserId = User.GetUserId(),
            Status = request.GetValueOrDefault("status"), Response = request.GetValueOrDefault("response")
        });
        return Ok(new { id });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(uint id)
    {
        await repository.ExecuteAsync(
            "UPDATE service_requests SET is_active=0,updated_at=UTC_TIMESTAMP(),updated_by=@UserId WHERE id=@Id AND parish_id=@ParishId",
            new { Id = id, UserId = User.GetUserId(), ParishId = User.GetParishId() });
        return NoContent();
    }
}
