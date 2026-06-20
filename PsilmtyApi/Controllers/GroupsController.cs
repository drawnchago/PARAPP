using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PsilmtyApi.Helpers;
using PsilmtyApi.Interfaces.Repositories;

namespace PsilmtyApi.Controllers;

[Authorize]
[ApiController]
[Route("api/grupos")]
public sealed class GroupsController(IDatabaseRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get() =>
        Ok(await repository.QueryAsync<object>("""
            SELECT g.id Id,g.name Name,g.description Description,g.parish_id ParishId,p.name ParishName,
                   CONCAT_WS(' ',u.first_name,u.last_name) LeaderName,
                   COUNT(DISTINCT gm.user_id) MemberCount,g.status IsActive
            FROM `groups` g
            LEFT JOIN parishes p ON p.id=g.parish_id
            LEFT JOIN users u ON u.id=g.leader_id
            LEFT JOIN group_members gm ON gm.group_id=g.id AND gm.status=1
            WHERE g.status=1 AND (@IsSuperAdmin=1 OR g.parish_id=@ParishId)
            GROUP BY g.id,p.name,u.first_name,u.last_name ORDER BY g.name
            """, new { IsSuperAdmin = User.IsSuperAdmin(), ParishId = User.GetParishId() }));

    [HttpPost]
    public async Task<IActionResult> Create(Dictionary<string, object?> request)
    {
        var id = await repository.ExecuteScalarAsync<uint>("""
            INSERT INTO `groups`(parish_id,name,description,leader_id,status,created_at,created_by)
            VALUES(@ParishId,@Name,@Description,@LeaderId,1,UTC_TIMESTAMP(),@UserId);
            SELECT LAST_INSERT_ID();
            """, new
        {
            ParishId = User.GetParishId(),
            Name = request.GetValueOrDefault("name"),
            Description = request.GetValueOrDefault("description"),
            LeaderId = request.GetValueOrDefault("leaderId"),
            UserId = User.GetUserId()
        });
        return Ok(new { id });
    }

    [HttpGet("{groupId:int}/miembros")]
    public async Task<IActionResult> GetMembers(uint groupId) =>
        Ok(await repository.QueryAsync<object>("""
            SELECT u.id UserId,u.first_name Nombre,u.last_name Apellido,u.email Correo,
                   u.mobile_phone Telefono,u.photo_url FotoUrl,gm.role_in_group RoleInGroup,gm.joined_at JoinedAt
            FROM group_members gm JOIN users u ON u.id=gm.user_id
            JOIN `groups` g ON g.id=gm.group_id
            WHERE gm.group_id=@GroupId AND gm.status=1
              AND (@IsSuperAdmin=1 OR g.parish_id=@ParishId)
            ORDER BY u.first_name,u.last_name
            """, new { GroupId = groupId, IsSuperAdmin = User.IsSuperAdmin(), ParishId = User.GetParishId() }));

    [HttpPost("{groupId:int}/miembros")]
    public async Task<IActionResult> AddMember(uint groupId, Dictionary<string, object?> request)
    {
        await repository.ExecuteAsync("""
            INSERT INTO group_members(group_id,user_id,role_in_group,joined_at,status,created_at,created_by)
            VALUES(@GroupId,@UserId,@Role,UTC_TIMESTAMP(),1,UTC_TIMESTAMP(),@CreatorId)
            ON DUPLICATE KEY UPDATE role_in_group=VALUES(role_in_group),status=1,updated_at=UTC_TIMESTAMP()
            """, new
        {
            GroupId = groupId,
            UserId = request.GetValueOrDefault("userId"),
            Role = request.GetValueOrDefault("roleInGroup") ?? "member",
            CreatorId = User.GetUserId()
        });
        return NoContent();
    }

    [HttpDelete("{groupId:int}/miembros/{userId:int}")]
    public async Task<IActionResult> RemoveMember(uint groupId, uint userId)
    {
        await repository.ExecuteAsync(
            "UPDATE group_members SET status=0,updated_at=UTC_TIMESTAMP(),updated_by=@EditorId WHERE group_id=@GroupId AND user_id=@UserId",
            new { GroupId = groupId, UserId = userId, EditorId = User.GetUserId() });
        return NoContent();
    }
}
