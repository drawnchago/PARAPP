namespace PsilmtyApi.Models.Requests;

public sealed class CountryCatalogRequest
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public bool IsActive { get; set; } = true;
}

public sealed class StateCatalogRequest
{
    public uint CountryId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public bool IsActive { get; set; } = true;
}

public sealed class NeighborhoodCatalogRequest
{
    public uint StateId { get; set; }
    public string PostalCode { get; set; } = "";
    public string Name { get; set; } = "";
    public string? SettlementType { get; set; }
    public string? Municipality { get; set; }
    public string? City { get; set; }
    public string? PostalStateCode { get; set; }
    public string? MunicipalityCode { get; set; }
    public string? SettlementCode { get; set; }
    public string? Zone { get; set; }
    public bool IsActive { get; set; } = true;
}
