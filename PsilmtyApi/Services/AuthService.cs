using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PsilmtyApi.Dictionaries;
using PsilmtyApi.Helpers;
using PsilmtyApi.Interfaces.Repositories;
using PsilmtyApi.Interfaces.Services;
using PsilmtyApi.Models.Options;
using PsilmtyApi.Models.Requests;
using PsilmtyApi.Models.Responses;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PsilmtyApi.Services;

public sealed class AuthService(
    IDatabaseRepository repository,
    IOptions<JwtOptions> jwtOptions) : IAuthService
{
    private readonly JwtOptions _jwt = jwtOptions.Value;

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await repository.QuerySingleAsync<AuthUserRow>("""
            SELECT u.id, u.parish_id ParishId, u.first_name FirstName, u.last_name LastName,
                   u.email, u.password_hash PasswordHash, u.photo_url PhotoUrl,
                   p.name ParishName, p.logo_url ParishLogo, u.is_active IsActive
            FROM users u
            LEFT JOIN parishes p ON p.id = u.parish_id
            WHERE LOWER(u.email) = LOWER(@Login)
            LIMIT 1
            """, new { Login = request.UsuarioOCorreo.Trim() });

        if (user is null || !user.IsActive || !PasswordHelper.Verify(request.Password, user.PasswordHash))
            return null;

        var roles = await GetRolesAsync(user.Id);
        var modules = await GetModulesAsync(user.Id, roles.Contains(RoleDictionary.SuperAdmin));
        var expiration = DateTime.UtcNow.AddMinutes(_jwt.ExpirationMinutes);

        await repository.ExecuteAsync(
            "UPDATE users SET last_login = UTC_TIMESTAMP() WHERE id = @Id",
            new { user.Id });

        return CreateResponse(user, roles, modules, expiration);
    }

    public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
    {
        if (request.Password.Length < 6) throw new InvalidOperationException("Password must contain at least six characters.");
        await ValidateLocationAsync(request.StateId, request.NeighborhoodId);

        var existing = await repository.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM users WHERE LOWER(email) = LOWER(@Email)",
            new { request.Email });
        if (existing > 0) throw new InvalidOperationException("The email address is already registered.");

        var userId = await repository.ExecuteScalarAsync<uint>("""
            INSERT INTO users
                (parish_id, first_name, last_name, second_last_name, email, password_hash,
                 mobile_phone, address, neighborhood, zip_code, city, state,
                 state_id, neighborhood_id, is_active, created_at)
            VALUES
                (@ParishId, @FirstName, @LastName, @SecondName, @Email, @PasswordHash,
                 @MobilePhone, @Address,
                 COALESCE((SELECT name FROM neighborhoods WHERE id=@NeighborhoodId),@Neighborhood),
                 COALESCE((SELECT postal_code FROM neighborhoods WHERE id=@NeighborhoodId),@ZipCode),
                 COALESCE((SELECT city FROM neighborhoods WHERE id=@NeighborhoodId),@City),
                 COALESCE((SELECT name FROM states WHERE id=@StateId),@State),
                 @StateId,@NeighborhoodId,1,UTC_TIMESTAMP());
            SELECT LAST_INSERT_ID();
            """, new
        {
            request.ParishId,
            request.FirstName,
            request.LastName,
            request.SecondName,
            request.Email,
            PasswordHash = PasswordHelper.Hash(request.Password),
            request.MobilePhone,
            request.Address,
            request.Neighborhood,
            request.StateId,
            request.NeighborhoodId,
            request.ZipCode,
            request.City,
            request.State
        });

        await repository.ExecuteAsync("""
            INSERT INTO user_roles (user_id, role_id, created_at)
            SELECT @UserId, id, UTC_TIMESTAMP()
            FROM roles WHERE name = @RoleName LIMIT 1
            """, new { UserId = userId, RoleName = RoleDictionary.Parishioner });

        await repository.ExecuteAsync("""
            INSERT INTO user_preferences (user_id, language_id, theme_id, created_at)
            SELECT @UserId,
                   (SELECT id FROM languages WHERE is_default = 1 LIMIT 1),
                   (SELECT id FROM themes WHERE is_default = 1 LIMIT 1),
                   UTC_TIMESTAMP()
            """, new { UserId = userId });

        return (await LoginAsync(new LoginRequest { UsuarioOCorreo = request.Email, Password = request.Password }))!;
    }

    private async Task ValidateLocationAsync(uint? stateId, uint? neighborhoodId)
    {
        if (!neighborhoodId.HasValue) return;
        if (!stateId.HasValue)
            throw new InvalidOperationException("A state is required when a neighborhood is selected.");

        var valid = await repository.ExecuteScalarAsync<int>("""
            SELECT COUNT(*) FROM neighborhoods
            WHERE id=@NeighborhoodId AND state_id=@StateId AND is_active=1
            """, new { StateId = stateId.Value, NeighborhoodId = neighborhoodId.Value });
        if (valid == 0)
            throw new InvalidOperationException("The selected neighborhood does not belong to the selected state.");
    }

    private async Task<HashSet<string>> GetRolesAsync(uint userId) =>
        (await repository.QueryAsync<string>("""
            SELECT r.name
            FROM roles r
            JOIN user_roles ur ON ur.role_id = r.id
            WHERE ur.user_id = @UserId AND r.is_active = 1
            """, new { UserId = userId })).ToHashSet(StringComparer.OrdinalIgnoreCase);

    private async Task<IReadOnlyList<ModulePermissionResponse>> GetModulesAsync(uint userId, bool superAdmin)
    {
        if (superAdmin)
            return await repository.QueryAsync<ModulePermissionResponse>("""
                SELECT id ModuloId, code Clave, name Nombre, icon Icono, route Ruta,
                       TRUE PuedeVer, TRUE PuedeCrear, TRUE PuedeEditar, TRUE PuedeEliminar
                FROM modules WHERE is_active = 1 ORDER BY sort_order, name
                """);

        return await repository.QueryAsync<ModulePermissionResponse>("""
            SELECT m.id ModuloId, m.code Clave, m.name Nombre, m.icon Icono, m.route Ruta,
                   MAX(p.code = 'view') PuedeVer,
                   MAX(p.code = 'create') PuedeCrear,
                   MAX(p.code = 'edit') PuedeEditar,
                   MAX(p.code = 'delete') PuedeEliminar
            FROM modules m
            JOIN permissions p ON p.module_id = m.id AND p.is_active = 1
            JOIN role_permissions rp ON rp.permission_id = p.id
            JOIN user_roles ur ON ur.role_id = rp.role_id AND ur.user_id = @UserId
            WHERE m.is_active = 1
            GROUP BY m.id, m.code, m.name, m.icon, m.route, m.sort_order
            HAVING PuedeVer = 1
            ORDER BY m.sort_order, m.name
            """, new { UserId = userId });
    }

    private LoginResponse CreateResponse(
        AuthUserRow user,
        HashSet<string> roles,
        IReadOnlyList<ModulePermissionResponse> modules,
        DateTime expiration)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new("parish_id", user.ParishId.ToString())
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(_jwt.Issuer, _jwt.Audience, claims, expires: expiration, signingCredentials: credentials);

        return new LoginResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expiracion = expiration,
            Usuario = new UserSessionResponse
            {
                Id = user.Id,
                Nombre = user.FirstName,
                Apellido = user.LastName,
                Correo = user.Email,
                NombreUsuario = user.Email,
                FotoUrl = user.PhotoUrl,
                ParishId = user.ParishId,
                ParishName = user.ParishName,
                IsSuperAdmin = roles.Contains(RoleDictionary.SuperAdmin),
                IsParishAdmin = roles.Contains(RoleDictionary.ParishAdmin) || roles.Contains(RoleDictionary.Priest),
                IsGroupLeader = roles.Contains(RoleDictionary.Coordinator)
            },
            Modulos = modules
        };
    }

    private sealed class AuthUserRow
    {
        public uint Id { get; set; }
        public uint ParishId { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string? PhotoUrl { get; set; }
        public string? ParishName { get; set; }
        public bool IsActive { get; set; }
    }
}
