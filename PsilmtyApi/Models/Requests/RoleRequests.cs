namespace PsilmtyApi.Models.Requests;

public sealed class RoleCatalogRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
