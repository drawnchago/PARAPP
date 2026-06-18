namespace PsilmtyApi.Models.Requests;

public sealed class LoginRequest
{
    public string UsuarioOCorreo { get; set; } = "";
    public string Password { get; set; } = "";
}

public sealed class RegisterRequest
{
    public string FirstName { get; set; } = "";
    public string? SecondName { get; set; }
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public uint ParishId { get; set; }
    public string? MobilePhone { get; set; }
    public string? Address { get; set; }
    public string? Neighborhood { get; set; }
    public uint? StateId { get; set; }
    public uint? NeighborhoodId { get; set; }
    public string? ZipCode { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
}
