using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PsilmtyApi.Dictionaries;
using PsilmtyApi.Helpers;
using PsilmtyApi.Interfaces.Repositories;
using PsilmtyApi.Models.Requests;

namespace PsilmtyApi.Controllers;

[Authorize]
[ApiController]
[Route("api/adoradores")]
public sealed class AdorersController(IDatabaseRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] bool soloActivos = false) =>
        Ok(await repository.QueryAsync<object>("""
            SELECT a.id Id,u.first_name Nombre,u.last_name Apellido,u.email Correo,
                   u.mobile_phone Telefono,a.notes Instrumento,a.is_active Activo,a.created_at CreatedAt
            FROM adorers a JOIN users u ON u.id=a.user_id
            WHERE a.parish_id=@ParishId AND (@OnlyActive=0 OR a.is_active=1)
            ORDER BY u.first_name,u.last_name
            """, new { ParishId = User.GetParishId(), OnlyActive = soloActivos }));

    [HttpPost]
    public async Task<IActionResult> Create(AdorerRequest request)
    {
        var userId = request.Email is null ? 0 : await repository.ExecuteScalarAsync<uint>(
            "SELECT COALESCE(MAX(id),0) FROM users WHERE LOWER(email)=LOWER(@Email)",
            new { request.Email });
        if (userId == 0)
        {
            userId = await repository.ExecuteScalarAsync<uint>("""
                INSERT INTO users
                    (parish_id,first_name,last_name,email,mobile_phone,password_hash,is_active,created_at,created_by)
                VALUES
                    (@ParishId,@FirstName,@LastName,@Email,@Phone,@PasswordHash,1,UTC_TIMESTAMP(),@CreatorId);
                SELECT LAST_INSERT_ID();
                """, new
            {
                ParishId = User.GetParishId(), request.FirstName, request.LastName,
                Email = request.Email ?? $"adorer-{Guid.NewGuid():N}@pending.local",
                request.Phone, PasswordHash = PasswordHelper.Hash(Guid.NewGuid().ToString("N")),
                CreatorId = User.GetUserId()
            });
            await repository.ExecuteAsync("""
                INSERT INTO user_roles(user_id,role_id,created_at,created_by)
                SELECT @UserId,id,UTC_TIMESTAMP(),@CreatorId FROM roles WHERE name=@Role LIMIT 1
                """, new { UserId = userId, CreatorId = User.GetUserId(), Role = RoleDictionary.Parishioner });
        }

        var id = await repository.ExecuteScalarAsync<uint>("""
            INSERT INTO adorers
                (user_id,parish_id,start_datetime,commitment_type,notes,is_active,created_at,created_by)
            VALUES
                (@TargetUserId,@ParishId,UTC_TIMESTAMP(),'permanent',@Instrument,1,UTC_TIMESTAMP(),@CreatorId);
            SELECT LAST_INSERT_ID();
            """, new
        {
            TargetUserId = userId, ParishId = User.GetParishId(),
            Instrument = request.Instrument, CreatorId = User.GetUserId()
        });
        return Ok(new { id });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(uint id)
    {
        await repository.ExecuteAsync(
            "UPDATE adorers SET is_active=0,updated_at=UTC_TIMESTAMP(),updated_by=@UserId WHERE id=@Id AND parish_id=@ParishId",
            new { Id = id, UserId = User.GetUserId(), ParishId = User.GetParishId() });
        return NoContent();
    }
}
