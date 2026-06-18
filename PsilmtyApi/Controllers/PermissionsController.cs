using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PsilmtyApi.Helpers;
using PsilmtyApi.Interfaces.Repositories;

namespace PsilmtyApi.Controllers;

[Authorize]
[ApiController]
[Route("api/permisos")]
public sealed class PermissionsController(IDatabaseRepository repository) : ControllerBase
{
    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles() =>
        Ok(await repository.QueryAsync<object>(
            "SELECT id Id,name Name,description Description,is_system IsSystem FROM roles WHERE is_active=1 ORDER BY name"));

    [HttpGet("modulos/buscar")]
    public async Task<IActionResult> SearchModules([FromQuery] string q = "") =>
        Ok(await repository.QueryAsync<object>("""
            SELECT id ModuleId,code Code,name Name,description Description,icon Icon,route Route
            FROM modules WHERE is_active=1 AND (@Query='' OR name LIKE CONCAT('%',@Query,'%'))
            ORDER BY sort_order,name
            """, new { Query = q }));

    [HttpGet("usuarios/{userId:int}/modulos")]
    public async Task<IActionResult> GetUserModules(uint userId) =>
        Ok(await repository.QueryAsync<object>("""
            SELECT m.id ModuleId,m.code Code,m.name Name,m.description Description,m.icon Icon,m.route Route,
                   MAX(p.code='view') CanView,MAX(p.code='create') CanCreate,
                   MAX(p.code='edit') CanEdit,MAX(p.code='delete') CanDelete
            FROM modules m
            JOIN permissions p ON p.module_id=m.id AND p.is_active=1
            JOIN role_permissions rp ON rp.permission_id=p.id
            JOIN user_roles ur ON ur.role_id=rp.role_id AND ur.user_id=@UserId
            WHERE m.is_active=1 GROUP BY m.id,m.code,m.name,m.description,m.icon,m.route,m.sort_order
            ORDER BY m.sort_order,m.name
            """, new { UserId = userId }));

    [HttpGet("modulos/{moduleId:int}/usuarios")]
    public async Task<IActionResult> GetUsersByModule(uint moduleId) =>
        Ok(await repository.QueryAsync<object>("""
            SELECT DISTINCT u.id Id,u.first_name Nombre,u.last_name Apellido,u.email Correo,u.photo_url FotoUrl
            FROM users u
            JOIN user_roles ur ON ur.user_id=u.id
            JOIN role_permissions rp ON rp.role_id=ur.role_id
            JOIN permissions p ON p.id=rp.permission_id AND p.module_id=@ModuleId AND p.code='view'
            WHERE u.is_active=1 AND (@IsSuperAdmin=1 OR u.parish_id=@ParishId)
            ORDER BY u.first_name,u.last_name
            """, new { ModuleId = moduleId, IsSuperAdmin = User.IsSuperAdmin(), ParishId = User.GetParishId() }));

    [HttpGet("usuarios/{userId:int}/accesos")]
    public Task<IActionResult> GetUserAccess(uint userId) => GetUserModules(userId);

    [HttpGet("modulos/{moduleId:int}/accesos")]
    public Task<IActionResult> GetModuleAccess(uint moduleId) => GetUsersByModule(moduleId);

    [HttpPost("usuarios/{userId:int}/modulos")]
    [HttpPost("usuarios/{userId:int}/accesos")]
    public IActionResult AssignIndividualAccess(uint userId) =>
        StatusCode(StatusCodes.Status501NotImplemented, new
        {
            message = "The current database schema only supports role permissions. Add a user_module_access table before enabling individual assignments."
        });

    [HttpDelete("usuarios/{userId:int}/accesos/{moduleId:int}")]
    public IActionResult RevokeIndividualAccess(uint userId, uint moduleId) =>
        StatusCode(StatusCodes.Status501NotImplemented, new
        {
            message = "The current database schema does not contain individual module access records."
        });
}
