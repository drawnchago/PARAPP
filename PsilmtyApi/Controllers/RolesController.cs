using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PsilmtyApi.Dictionaries;
using PsilmtyApi.Helpers;
using PsilmtyApi.Interfaces.Repositories;
using PsilmtyApi.Models.Requests;

namespace PsilmtyApi.Controllers;

[Authorize(Roles = RoleDictionary.SuperAdmin)]
[ApiController]
[Route("api/catalogos/admin/roles")]
public sealed class RolesController(IDatabaseRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string q = "", [FromQuery] bool includeInactive = true) =>
        Ok(await repository.QueryAsync<object>("""
            SELECT id Id, name Name, description Description, is_system IsSystem,
                   status IsActive, created_at CreatedAt, updated_at UpdatedAt
            FROM roles
            WHERE (@IncludeInactive=1 OR status=1)
              AND (@Query='' OR name LIKE CONCAT('%',@Query,'%') OR description LIKE CONCAT('%',@Query,'%'))
            ORDER BY is_system DESC, name
            """, new { Query = q.Trim(), IncludeInactive = includeInactive }));

    [HttpPost]
    public async Task<IActionResult> Create(RoleCatalogRequest request)
    {
        var name = (request.Name ?? "").Trim();
        if (name.Length == 0) return BadRequest(new { message = "El nombre es requerido." });

        var exists = await repository.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM roles WHERE name=@Name", new { Name = name });
        if (exists > 0) return BadRequest(new { message = "Ya existe un rol con ese nombre." });

        var id = await repository.ExecuteScalarAsync<uint>("""
            INSERT INTO roles(name, description, is_system, status, created_at, created_by)
            VALUES(@Name, @Description, 0, @IsActive, UTC_TIMESTAMP(), @UserId);
            SELECT LAST_INSERT_ID();
            """, new { Name = name, request.Description, request.IsActive, UserId = User.GetUserId() });
        return Ok(new { id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(uint id, RoleCatalogRequest request)
    {
        if (await IsSystemAsync(id))
            return BadRequest(new { message = "Los roles del sistema no se pueden modificar." });

        var name = (request.Name ?? "").Trim();
        if (name.Length == 0) return BadRequest(new { message = "El nombre es requerido." });

        var dup = await repository.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM roles WHERE name=@Name AND id<>@Id", new { Name = name, Id = id });
        if (dup > 0) return BadRequest(new { message = "Ya existe un rol con ese nombre." });

        await repository.ExecuteAsync("""
            UPDATE roles SET name=@Name, description=@Description, status=@IsActive,
                updated_at=UTC_TIMESTAMP(), updated_by=@UserId
            WHERE id=@Id
            """, new { Id = id, Name = name, request.Description, request.IsActive, UserId = User.GetUserId() });
        return Ok(new { id });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deactivate(uint id)
    {
        if (await IsSystemAsync(id))
            return BadRequest(new { message = "Los roles del sistema no se pueden desactivar." });

        await repository.ExecuteAsync(
            "UPDATE roles SET status=0, updated_at=UTC_TIMESTAMP(), updated_by=@UserId WHERE id=@Id",
            new { Id = id, UserId = User.GetUserId() });
        return Ok(new { id });
    }

    private async Task<bool> IsSystemAsync(uint id) =>
        await repository.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM roles WHERE id=@Id AND is_system=1", new { Id = id }) > 0;
}
