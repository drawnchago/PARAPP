namespace PsilmtyApi.Models.Requests;

public sealed class UserPreferencesRequest
{
    public uint? LenguajeId { get; set; }
    public uint? TemaId { get; set; }
    public bool? NotifPushActivas { get; set; }
    public bool? NotifEventos { get; set; }
    public bool? NotifNoticias { get; set; }
    public bool? NotifRecordatorios { get; set; }
    public bool? NotifAdoracion { get; set; }
    public bool? NotifSonido { get; set; }
    public bool? NotifVibracion { get; set; }
    public string? CalendarioVista { get; set; }
    public byte? CalendarioPrimerDia { get; set; }
    public string? TamanoFuente { get; set; }
    public bool? AltoContraste { get; set; }
}

public sealed class ProfileRequest
{
    public string? FirstName { get; set; }
    public string? SecondName { get; set; }
    public string? LastName { get; set; }
    public string? MobilePhone { get; set; }
    public string? Address { get; set; }
    public string? Neighborhood { get; set; }
    public uint? StateId { get; set; }
    public uint? NeighborhoodId { get; set; }
    public string? ZipCode { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
}

public sealed class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = "";
    public string NewPassword { get; set; } = "";
}

public sealed class SaveUserRequest
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? SecondLastName { get; set; }
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string? MobilePhone { get; set; }
    public string Gender { get; set; } = "unspecified";
    public string RoleName { get; set; } = "parishioner";
    public uint? ParishId { get; set; }
    public string? Address { get; set; }
    public uint? StateId { get; set; }
    public uint? NeighborhoodId { get; set; }
    public string? ZipCode { get; set; }
    public string? City { get; set; }
}
