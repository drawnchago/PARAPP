namespace PsilmtyApi.Models.Responses;

public sealed class CountryResponse
{
    public uint Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class StateResponse
{
    public uint Id { get; set; }
    public uint CountryId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string CountryName { get; set; } = "";
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class NeighborhoodResponse
{
    public uint Id { get; set; }
    public uint StateId { get; set; }
    public string PostalCode { get; set; } = "";
    public string Name { get; set; } = "";
    public string? SettlementType { get; set; }
    public string? Municipality { get; set; }
    public string? City { get; set; }
    public string StateName { get; set; } = "";
    public string CountryName { get; set; } = "";
    public string? PostalStateCode { get; set; }
    public string? MunicipalityCode { get; set; }
    public string? SettlementCode { get; set; }
    public string? Zone { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string Display => string.IsNullOrWhiteSpace(Municipality)
        ? $"{Name} — {PostalCode}"
        : $"{Name} — {Municipality}, {PostalCode}";
}
