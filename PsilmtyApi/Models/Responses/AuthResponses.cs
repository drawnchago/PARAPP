namespace PsilmtyApi.Models.Responses;

public sealed class LoginResponse
{
    public string Token { get; set; } = "";
    public DateTime Expiracion { get; set; }
    public UserSessionResponse Usuario { get; set; } = new();
    public IReadOnlyList<ModulePermissionResponse> Modulos { get; set; } = [];
}

public sealed class UserSessionResponse
{
    public uint Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Apellido { get; set; } = "";
    public string Correo { get; set; } = "";
    public string NombreUsuario { get; set; } = "";
    public string? FotoUrl { get; set; }
    public uint ParishId { get; set; }
    public string? ParishName { get; set; }
    public string? ParishColor { get; set; }
    public string? ParishColorSec { get; set; }
    public bool IsSuperAdmin { get; set; }
    public bool IsParishAdmin { get; set; }
    public bool IsGroupLeader { get; set; }
}

public sealed class ModulePermissionResponse
{
    public uint ModuloId { get; set; }
    public string Clave { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string? Icono { get; set; }
    public string? Ruta { get; set; }
    public bool PuedeVer { get; set; }
    public bool PuedeCrear { get; set; }
    public bool PuedeEditar { get; set; }
    public bool PuedeEliminar { get; set; }
}
