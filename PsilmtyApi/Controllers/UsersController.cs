using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PsilmtyApi.Helpers;
using PsilmtyApi.Interfaces.Repositories;
using PsilmtyApi.Models.Requests;

namespace PsilmtyApi.Controllers;

[Authorize]
[ApiController]
[Route("api/usuarios")]
public sealed class UsersController(IDatabaseRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string q = "", [FromQuery] uint? parishId = null, [FromQuery] uint? groupId = null)
    {
        var effectiveParishId = User.IsSuperAdmin() ? parishId : User.GetParishId();
        return Ok(await repository.QueryAsync<object>("""
            SELECT u.id Id, u.first_name Nombre, u.last_name Apellido, u.second_last_name Apellido2,
                   u.email Correo, u.mobile_phone Telefono, u.photo_url FotoUrl, u.status Activo,
                   u.parish_id ParishId, p.name ParishName, u.created_at CreatedAt, u.last_login LastLogin,
                   u.address Address,u.zip_code ZipCode,u.city City,
                   u.state_id StateId,u.neighborhood_id NeighborhoodId,s.country_id CountryId,
                   s.name StateName,n.name NeighborhoodName,
                   GROUP_CONCAT(DISTINCT r.name ORDER BY r.name) RolesCsv
            FROM users u
            LEFT JOIN parishes p ON p.id=u.parish_id
            LEFT JOIN states s ON s.id=u.state_id
            LEFT JOIN neighborhoods n ON n.id=u.neighborhood_id
            LEFT JOIN user_roles ur ON ur.user_id=u.id
            LEFT JOIN roles r ON r.id=ur.role_id
            LEFT JOIN group_members gm ON gm.user_id=u.id AND gm.status=1
            WHERE (@ParishId IS NULL OR u.parish_id=@ParishId)
              AND (@GroupId IS NULL OR gm.group_id=@GroupId)
              AND (@Query='' OR CONCAT_WS(' ',u.first_name,u.last_name,u.email) LIKE CONCAT('%',@Query,'%'))
            GROUP BY u.id, p.name
            ORDER BY u.first_name,u.last_name
            """, new { ParishId = effectiveParishId, GroupId = groupId, Query = q }));
    }

    [HttpGet("buscar")]
    public async Task<IActionResult> Search([FromQuery] string q) =>
        Ok(await repository.QueryAsync<object>("""
            SELECT id Id, first_name Nombre, last_name Apellido, second_last_name Apellido2,
                   email Correo, mobile_phone Telefono, photo_url FotoUrl
            FROM users
            WHERE status=1 AND parish_id=@ParishId
              AND CONCAT_WS(' ',first_name,last_name,email) LIKE CONCAT('%',@Query,'%')
            ORDER BY first_name,last_name LIMIT 30
            """, new { ParishId = User.GetParishId(), Query = q }));

    [HttpGet("perfil")]
    public async Task<IActionResult> GetProfile() =>
        Ok(await repository.QuerySingleAsync<object>("""
            SELECT u.id Id, u.first_name Nombre, u.last_name Apellido,
                   u.second_last_name Apellido2, u.email Correo,
                   u.mobile_phone Telefono, u.home_phone Telefono2,
                   u.address Direccion, u.city Ciudad, u.state Estado, p.name ParishName,
                   u.zip_code ZipCode,u.state_id StateId,u.neighborhood_id NeighborhoodId,
                   s.country_id CountryId,n.name NeighborhoodName
            FROM users u LEFT JOIN parishes p ON p.id=u.parish_id
            LEFT JOIN states s ON s.id=u.state_id
            LEFT JOIN neighborhoods n ON n.id=u.neighborhood_id
            WHERE u.id=@UserId
            """, new { UserId = User.GetUserId() }));

    [HttpPut("perfil")]
    public async Task<IActionResult> UpdateProfile(ProfileRequest request)
    {
        if (!await IsLocationValidAsync(request.StateId, request.NeighborhoodId))
            return BadRequest(new { message = "The selected neighborhood does not belong to the selected state." });

        await repository.ExecuteAsync("""
            UPDATE users SET first_name=COALESCE(@FirstName,first_name),
                second_last_name=COALESCE(@SecondName,second_last_name),
                last_name=COALESCE(@LastName,last_name),
                mobile_phone=COALESCE(@MobilePhone,mobile_phone),
                address=COALESCE(@Address,address),
                neighborhood=COALESCE((SELECT name FROM neighborhoods WHERE id=@NeighborhoodId),@Neighborhood,neighborhood),
                zip_code=COALESCE((SELECT postal_code FROM neighborhoods WHERE id=@NeighborhoodId),@ZipCode,zip_code),
                city=COALESCE((SELECT city FROM neighborhoods WHERE id=@NeighborhoodId),@City,city),
                state=COALESCE((SELECT name FROM states WHERE id=@StateId),@State,state),
                state_id=COALESCE(@StateId,state_id),neighborhood_id=COALESCE(@NeighborhoodId,neighborhood_id),
                updated_at=UTC_TIMESTAMP(), updated_by=@UserId
            WHERE id=@UserId
            """, new
        {
            UserId = User.GetUserId(), request.FirstName, request.SecondName, request.LastName,
            request.MobilePhone, request.Address, request.Neighborhood, request.ZipCode,
            request.City, request.State, request.StateId, request.NeighborhoodId
        });
        return await GetProfile();
    }

    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var hash = await repository.QuerySingleAsync<string>(
            "SELECT password_hash FROM users WHERE id=@UserId",
            new { UserId = User.GetUserId() });
        if (hash is null || !PasswordHelper.Verify(request.CurrentPassword, hash))
            return BadRequest(new { message = "Current password is invalid." });
        if (request.NewPassword.Length < 6)
            return BadRequest(new { message = "New password must contain at least six characters." });

        await repository.ExecuteAsync(
            "UPDATE users SET password_hash=@Hash, updated_at=UTC_TIMESTAMP(), updated_by=@UserId WHERE id=@UserId",
            new { Hash = PasswordHelper.Hash(request.NewPassword), UserId = User.GetUserId() });
        return NoContent();
    }

    [HttpPost]
    public async Task<IActionResult> Create(SaveUserRequest request)
    {
        if (!await IsLocationValidAsync(request.StateId, request.NeighborhoodId))
            return BadRequest(new { message = "The selected neighborhood does not belong to the selected state." });

        var parishId = User.IsSuperAdmin() && request.ParishId.HasValue ? request.ParishId.Value : User.GetParishId();
        var id = await repository.ExecuteScalarAsync<uint>("""
            INSERT INTO users
                (parish_id,first_name,last_name,second_last_name,email,password_hash,mobile_phone,gender,
                 address,state_id,neighborhood_id,state,neighborhood,zip_code,city,status,created_at,created_by)
            VALUES
                (@ParishId,@FirstName,@LastName,@SecondLastName,@Email,@Hash,@MobilePhone,@Gender,
                 @Address,@StateId,@NeighborhoodId,
                 (SELECT name FROM states WHERE id=@StateId),
                 (SELECT name FROM neighborhoods WHERE id=@NeighborhoodId),
                 (SELECT postal_code FROM neighborhoods WHERE id=@NeighborhoodId),
                 COALESCE(@City,(SELECT city FROM neighborhoods WHERE id=@NeighborhoodId)),
                 1,UTC_TIMESTAMP(),@CreatorId);
            SELECT LAST_INSERT_ID();
            """, new
        {
            ParishId = parishId, request.FirstName, request.LastName, request.SecondLastName,
            request.Email, Hash = PasswordHelper.Hash(request.Password), request.MobilePhone,
            request.Gender, request.Address, request.StateId, request.NeighborhoodId,
            request.City, CreatorId = User.GetUserId()
        });
        await SaveRoleAsync(id, request.RoleName);
        return Ok(new { id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(uint id, SaveUserRequest request)
    {
        if (!await IsLocationValidAsync(request.StateId, request.NeighborhoodId))
            return BadRequest(new { message = "The selected neighborhood does not belong to the selected state." });

        await repository.ExecuteAsync("""
            UPDATE users SET first_name=@FirstName,last_name=@LastName,second_last_name=@SecondLastName,
                email=@Email,mobile_phone=@MobilePhone,gender=@Gender,
                address=@Address,state_id=@StateId,neighborhood_id=@NeighborhoodId,
                state=(SELECT name FROM states WHERE id=@StateId),
                neighborhood=(SELECT name FROM neighborhoods WHERE id=@NeighborhoodId),
                zip_code=(SELECT postal_code FROM neighborhoods WHERE id=@NeighborhoodId),
                city=COALESCE(@City,(SELECT city FROM neighborhoods WHERE id=@NeighborhoodId)),
                password_hash=IF(@Password='',password_hash,@Hash),
                updated_at=UTC_TIMESTAMP(),updated_by=@EditorId
            WHERE id=@Id AND (@IsSuperAdmin=1 OR parish_id=@ParishId)
            """, new
        {
            Id = id, request.FirstName, request.LastName, request.SecondLastName, request.Email,
            request.MobilePhone, request.Gender, request.Password,
            request.Address, request.StateId, request.NeighborhoodId, request.City,
            Hash = string.IsNullOrEmpty(request.Password) ? "" : PasswordHelper.Hash(request.Password),
            EditorId = User.GetUserId(), IsSuperAdmin = User.IsSuperAdmin(), ParishId = User.GetParishId()
        });
        await SaveRoleAsync(id, request.RoleName);
        return Ok(new { id });
    }

    private async Task<bool> IsLocationValidAsync(uint? stateId, uint? neighborhoodId)
    {
        if (!neighborhoodId.HasValue) return true;
        if (!stateId.HasValue) return false;
        return await repository.ExecuteScalarAsync<int>("""
            SELECT COUNT(*) FROM neighborhoods
            WHERE id=@NeighborhoodId AND state_id=@StateId AND status=1
            """, new { StateId = stateId.Value, NeighborhoodId = neighborhoodId.Value }) == 1;
    }

    private async Task SaveRoleAsync(uint userId, string roleName)
    {
        await repository.ExecuteAsync("DELETE FROM user_roles WHERE user_id=@UserId", new { UserId = userId });
        await repository.ExecuteAsync("""
            INSERT INTO user_roles(user_id,role_id,created_at,created_by)
            SELECT @UserId,id,UTC_TIMESTAMP(),@CreatorId FROM roles WHERE name=@RoleName LIMIT 1
            """, new { UserId = userId, RoleName = roleName, CreatorId = User.GetUserId() });
    }
}
