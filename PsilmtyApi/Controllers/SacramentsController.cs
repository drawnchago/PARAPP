using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PsilmtyApi.Helpers;
using PsilmtyApi.Interfaces.Repositories;

namespace PsilmtyApi.Controllers;

[Authorize]
[ApiController]
[Route("api/sacramentos")]
public sealed class SacramentsController(IDatabaseRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await repository.QueryAsync<object>("""
        SELECT s.id Id,s.user_id UserId,CONCAT_WS(' ',u.first_name,u.last_name) UserName,
               s.type_id TypeId,t.name TypeName,s.date Date,s.minister Minister,
               s.godfather Godfather,s.godmother Godmother,s.record_number RecordNumber,
               s.observations Observations,s.document_url DocumentUrl
        FROM sacraments s JOIN users u ON u.id=s.user_id
        JOIN sacrament_types t ON t.id=s.type_id
        WHERE s.parish_id=@ParishId AND s.status=1 ORDER BY s.date DESC
        """, new { ParishId = User.GetParishId() }));

    [HttpGet("tipos")]
    public async Task<IActionResult> GetTypes() =>
        Ok(await repository.QueryAsync<object>("SELECT id Id,name Name FROM sacrament_types WHERE status=1 ORDER BY name"));

    [HttpPost]
    public async Task<IActionResult> Create(Dictionary<string, object?> request)
    {
        var id = await repository.ExecuteScalarAsync<uint>("""
            INSERT INTO sacraments
                (parish_id,user_id,type_id,date,minister,godfather,godmother,record_number,observations,status,created_at,created_by)
            VALUES
                (@ParishId,@TargetUserId,@TypeId,@Date,@Minister,@Godfather,@Godmother,@RecordNumber,@Observations,1,UTC_TIMESTAMP(),@UserId);
            SELECT LAST_INSERT_ID();
            """, new
        {
            ParishId = User.GetParishId(), TargetUserId = request.GetValueOrDefault("userId"),
            TypeId = request.GetValueOrDefault("typeId"), Date = request.GetValueOrDefault("date"),
            Minister = request.GetValueOrDefault("minister"), Godfather = request.GetValueOrDefault("godfather"),
            Godmother = request.GetValueOrDefault("godmother"), RecordNumber = request.GetValueOrDefault("recordNumber"),
            Observations = request.GetValueOrDefault("observations"), UserId = User.GetUserId()
        });
        return Ok(new { id });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(uint id)
    {
        await repository.ExecuteAsync(
            "UPDATE sacraments SET status=0,updated_at=UTC_TIMESTAMP(),updated_by=@UserId WHERE id=@Id AND parish_id=@ParishId",
            new { Id = id, UserId = User.GetUserId(), ParishId = User.GetParishId() });
        return NoContent();
    }
}
