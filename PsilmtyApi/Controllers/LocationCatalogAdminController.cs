using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PsilmtyApi.Dictionaries;
using PsilmtyApi.Helpers;
using PsilmtyApi.Interfaces.Repositories;
using PsilmtyApi.Models.Requests;
using PsilmtyApi.Models.Responses;

namespace PsilmtyApi.Controllers;

[Authorize(Roles = RoleDictionary.SuperAdmin)]
[ApiController]
[Route("api/catalogos/admin")]
public sealed class LocationCatalogAdminController(IDatabaseRepository repository) : ControllerBase
{
    [HttpGet("paises")]
    public async Task<IActionResult> GetCountries([FromQuery] string q = "", [FromQuery] bool includeInactive = true) =>
        Ok(await repository.QueryAsync<CountryResponse>("""
            SELECT id Id,code Code,name Name,status IsActive,created_at CreatedAt,updated_at UpdatedAt
            FROM countries
            WHERE (@IncludeInactive=1 OR status=1)
              AND (@Query='' OR name LIKE CONCAT('%',@Query,'%') OR code LIKE CONCAT('%',@Query,'%'))
            ORDER BY name
            """, new { Query = q.Trim(), IncludeInactive = includeInactive }));

    [HttpPost("paises")]
    public async Task<IActionResult> CreateCountry(CountryCatalogRequest request)
    {
        Normalize(request);
        var id = await repository.ExecuteScalarAsync<uint>("""
            INSERT INTO countries(code,name,status,created_at,created_by)
            VALUES(@Code,@Name,@IsActive,UTC_TIMESTAMP(),@UserId);
            SELECT LAST_INSERT_ID();
            """, new { request.Code, request.Name, request.IsActive, UserId = User.GetUserId() });
        return Ok(new { id });
    }

    [HttpPut("paises/{id:int}")]
    public async Task<IActionResult> UpdateCountry(uint id, CountryCatalogRequest request)
    {
        Normalize(request);
        await repository.ExecuteAsync("""
            UPDATE countries SET code=@Code,name=@Name,status=@IsActive,
                updated_at=UTC_TIMESTAMP(),updated_by=@UserId WHERE id=@Id
            """, new { Id = id, request.Code, request.Name, request.IsActive, UserId = User.GetUserId() });
        return Ok(new { id });
    }

    [HttpDelete("paises/{id:int}")]
    public Task<IActionResult> DeactivateCountry(uint id) =>
        SetStatusAsync("countries", id, false);

    [HttpGet("estados")]
    public async Task<IActionResult> GetStates(
        [FromQuery] uint? countryId = null,
        [FromQuery] string q = "",
        [FromQuery] bool includeInactive = true) =>
        Ok(await repository.QueryAsync<StateResponse>("""
            SELECT s.id Id,s.country_id CountryId,s.code Code,s.name Name,c.name CountryName,
                   s.status IsActive,s.created_at CreatedAt,s.updated_at UpdatedAt
            FROM states s JOIN countries c ON c.id=s.country_id
            WHERE (@CountryId IS NULL OR s.country_id=@CountryId)
              AND (@IncludeInactive=1 OR s.status=1)
              AND (@Query='' OR s.name LIKE CONCAT('%',@Query,'%') OR s.code LIKE CONCAT('%',@Query,'%'))
            ORDER BY c.name,s.name
            """, new { CountryId = countryId, Query = q.Trim(), IncludeInactive = includeInactive }));

    [HttpPost("estados")]
    public async Task<IActionResult> CreateState(StateCatalogRequest request)
    {
        Normalize(request);
        await EnsureActiveCountryAsync(request.CountryId);
        var id = await repository.ExecuteScalarAsync<uint>("""
            INSERT INTO states(country_id,code,name,status,created_at,created_by)
            VALUES(@CountryId,@Code,@Name,@IsActive,UTC_TIMESTAMP(),@UserId);
            SELECT LAST_INSERT_ID();
            """, new { request.CountryId, request.Code, request.Name, request.IsActive, UserId = User.GetUserId() });
        return Ok(new { id });
    }

    [HttpPut("estados/{id:int}")]
    public async Task<IActionResult> UpdateState(uint id, StateCatalogRequest request)
    {
        Normalize(request);
        await EnsureActiveCountryAsync(request.CountryId);
        await repository.ExecuteAsync("""
            UPDATE states SET country_id=@CountryId,code=@Code,name=@Name,status=@IsActive,
                updated_at=UTC_TIMESTAMP(),updated_by=@UserId WHERE id=@Id
            """, new { Id = id, request.CountryId, request.Code, request.Name, request.IsActive, UserId = User.GetUserId() });
        return Ok(new { id });
    }

    [HttpDelete("estados/{id:int}")]
    public Task<IActionResult> DeactivateState(uint id) =>
        SetStatusAsync("states", id, false);

    [HttpGet("colonias")]
    public async Task<IActionResult> GetNeighborhoods(
        [FromQuery] uint? stateId = null,
        [FromQuery] string q = "",
        [FromQuery] string postalCode = "",
        [FromQuery] bool includeInactive = true) =>
        Ok(await repository.QueryAsync<NeighborhoodResponse>("""
            SELECT n.id Id,n.state_id StateId,n.postal_code PostalCode,n.name Name,
                   n.settlement_type SettlementType,n.municipality Municipality,n.city City,
                   s.name StateName,c.name CountryName,n.postal_state_code PostalStateCode,
                   n.municipality_code MunicipalityCode,n.settlement_code SettlementCode,n.zone Zone,
                   n.status IsActive,n.created_at CreatedAt,n.updated_at UpdatedAt
            FROM neighborhoods n
            JOIN states s ON s.id=n.state_id
            JOIN countries c ON c.id=s.country_id
            WHERE (@StateId IS NULL OR n.state_id=@StateId)
              AND (@IncludeInactive=1 OR n.status=1)
              AND (@PostalCode='' OR n.postal_code=@PostalCode)
              AND (@Query='' OR n.name LIKE CONCAT('%',@Query,'%')
                   OR n.municipality LIKE CONCAT('%',@Query,'%')
                   OR n.city LIKE CONCAT('%',@Query,'%'))
            ORDER BY c.name,s.name,n.name,n.postal_code
            LIMIT 1000
            """, new
        {
            StateId = stateId, Query = q.Trim(), PostalCode = postalCode.Trim(),
            IncludeInactive = includeInactive
        }));

    [HttpPost("colonias")]
    public async Task<IActionResult> CreateNeighborhood(NeighborhoodCatalogRequest request)
    {
        Normalize(request);
        await EnsureActiveStateAsync(request.StateId);
        var id = await repository.ExecuteScalarAsync<uint>("""
            INSERT INTO neighborhoods
                (state_id,postal_code,name,settlement_type,municipality,city,postal_state_code,
                 municipality_code,settlement_code,zone,status,created_at,created_by)
            VALUES
                (@StateId,@PostalCode,@Name,@SettlementType,@Municipality,@City,@PostalStateCode,
                 @MunicipalityCode,@SettlementCode,@Zone,@IsActive,UTC_TIMESTAMP(),@UserId);
            SELECT LAST_INSERT_ID();
            """, new
        {
            request.StateId, request.PostalCode, request.Name, request.SettlementType,
            request.Municipality, request.City, request.PostalStateCode,
            request.MunicipalityCode, request.SettlementCode, request.Zone,
            request.IsActive, UserId = User.GetUserId()
        });
        return Ok(new { id });
    }

    [HttpPut("colonias/{id:int}")]
    public async Task<IActionResult> UpdateNeighborhood(uint id, NeighborhoodCatalogRequest request)
    {
        Normalize(request);
        await EnsureActiveStateAsync(request.StateId);
        await repository.ExecuteAsync("""
            UPDATE neighborhoods SET state_id=@StateId,postal_code=@PostalCode,name=@Name,
                settlement_type=@SettlementType,municipality=@Municipality,city=@City,
                postal_state_code=@PostalStateCode,municipality_code=@MunicipalityCode,
                settlement_code=@SettlementCode,zone=@Zone,status=@IsActive,
                updated_at=UTC_TIMESTAMP(),updated_by=@UserId WHERE id=@Id
            """, new
        {
            Id = id, request.StateId, request.PostalCode, request.Name, request.SettlementType,
            request.Municipality, request.City, request.PostalStateCode,
            request.MunicipalityCode, request.SettlementCode, request.Zone,
            request.IsActive, UserId = User.GetUserId()
        });
        return Ok(new { id });
    }

    [HttpDelete("colonias/{id:int}")]
    public Task<IActionResult> DeactivateNeighborhood(uint id) =>
        SetStatusAsync("neighborhoods", id, false);

    private async Task<IActionResult> SetStatusAsync(string table, uint id, bool active)
    {
        if (table is not ("countries" or "states" or "neighborhoods")) return BadRequest();
        await repository.ExecuteAsync(
            $"UPDATE {table} SET status=@Active,updated_at=UTC_TIMESTAMP(),updated_by=@UserId WHERE id=@Id",
            new { Id = id, Active = active, UserId = User.GetUserId() });
        return NoContent();
    }

    private async Task EnsureActiveCountryAsync(uint id)
    {
        if (await repository.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM countries WHERE id=@Id AND status=1", new { Id = id }) != 1)
            throw new InvalidOperationException("The selected country is not active.");
    }

    private async Task EnsureActiveStateAsync(uint id)
    {
        if (await repository.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM states WHERE id=@Id AND status=1", new { Id = id }) != 1)
            throw new InvalidOperationException("The selected state is not active.");
    }

    private static void Normalize(CountryCatalogRequest request)
    {
        request.Code = request.Code.Trim().ToUpperInvariant();
        request.Name = request.Name.Trim();
        if (request.Code.Length != 3 || request.Name.Length == 0)
            throw new InvalidOperationException("Country code must contain three characters and name is required.");
    }

    private static void Normalize(StateCatalogRequest request)
    {
        request.Code = request.Code.Trim().ToUpperInvariant();
        request.Name = request.Name.Trim();
        if (request.CountryId == 0 || request.Code.Length == 0 || request.Name.Length == 0)
            throw new InvalidOperationException("Country, state code, and state name are required.");
    }

    private static void Normalize(NeighborhoodCatalogRequest request)
    {
        request.PostalCode = request.PostalCode.Trim();
        request.Name = request.Name.Trim();
        if (request.StateId == 0 || request.PostalCode.Length == 0 || request.Name.Length == 0)
            throw new InvalidOperationException("State, postal code, and neighborhood name are required.");
    }
}
