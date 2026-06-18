using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PsilmtyApi.Helpers;
using PsilmtyApi.Interfaces.Repositories;

namespace PsilmtyApi.Controllers;

[Authorize]
[ApiController]
[Route("api/intenciones")]
public sealed class MassIntentionsController(IDatabaseRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await repository.QueryAsync<object>("""
        SELECT i.id Id,i.user_id UserId,CONCAT_WS(' ',u.first_name,u.last_name) UserName,
               i.intention Intention,i.type Type,i.mass_date MassDate,i.status Status,
               i.donation Donation,i.created_at CreatedAt
        FROM mass_intentions i LEFT JOIN users u ON u.id=i.user_id
        WHERE i.parish_id=@ParishId AND i.is_active=1 ORDER BY i.created_at DESC
        """, new { ParishId = User.GetParishId() }));

    [HttpPost]
    public async Task<IActionResult> Create(Dictionary<string, object?> request)
    {
        var id = await repository.ExecuteScalarAsync<uint>("""
            INSERT INTO mass_intentions
                (parish_id,user_id,intention,type,mass_date,status,donation,is_active,created_at,created_by)
            VALUES
                (@ParishId,COALESCE(@TargetUserId,@UserId),@Intention,@Type,@MassDate,'pending',@Donation,1,UTC_TIMESTAMP(),@UserId);
            SELECT LAST_INSERT_ID();
            """, new
        {
            ParishId = User.GetParishId(), TargetUserId = request.GetValueOrDefault("userId"),
            UserId = User.GetUserId(), Intention = request.GetValueOrDefault("intention"),
            Type = request.GetValueOrDefault("type") ?? "petition",
            MassDate = request.GetValueOrDefault("massDate"), Donation = request.GetValueOrDefault("donation")
        });
        return Ok(new { id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(uint id, Dictionary<string, object?> request)
    {
        await repository.ExecuteAsync("""
            UPDATE mass_intentions SET intention=@Intention,type=@Type,mass_date=@MassDate,
                donation=@Donation,updated_at=UTC_TIMESTAMP(),updated_by=@UserId
            WHERE id=@Id AND parish_id=@ParishId
            """, new
        {
            Id = id, ParishId = User.GetParishId(), UserId = User.GetUserId(),
            Intention = request.GetValueOrDefault("intention"), Type = request.GetValueOrDefault("type"),
            MassDate = request.GetValueOrDefault("massDate"), Donation = request.GetValueOrDefault("donation")
        });
        return Ok(new { id });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(uint id)
    {
        await repository.ExecuteAsync(
            "UPDATE mass_intentions SET is_active=0,updated_at=UTC_TIMESTAMP(),updated_by=@UserId WHERE id=@Id AND parish_id=@ParishId",
            new { Id = id, UserId = User.GetUserId(), ParishId = User.GetParishId() });
        return NoContent();
    }
}
