using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PsilmtyApi.Interfaces.Repositories;
using PsilmtyApi.Models.Responses;

namespace PsilmtyApi.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/catalogos")]
public sealed class CatalogsController(IDatabaseRepository repository) : ControllerBase
{
    [HttpGet("paises")]
    public async Task<IActionResult> GetCountries() =>
        Ok(await repository.QueryAsync<CountryResponse>("""
            SELECT id Id,code Code,name Name
            FROM countries WHERE status=1 ORDER BY name
            """));

    [HttpGet("estados")]
    public async Task<IActionResult> GetStates([FromQuery] uint countryId) =>
        Ok(await repository.QueryAsync<StateResponse>("""
            SELECT id Id,country_id CountryId,code Code,name Name
            FROM states
            WHERE country_id=@CountryId AND status=1
            ORDER BY name
            """, new { CountryId = countryId }));

    [HttpGet("colonias")]
    public async Task<IActionResult> GetNeighborhoods(
        [FromQuery] uint stateId,
        [FromQuery] string q = "",
        [FromQuery] string postalCode = "")
    {
        var query = q.Trim();
        var zipCode = postalCode.Trim();
        return Ok(await repository.QueryAsync<NeighborhoodResponse>("""
            SELECT id Id,state_id StateId,postal_code PostalCode,name Name,
                   settlement_type SettlementType,municipality Municipality,city City
            FROM neighborhoods
            WHERE state_id=@StateId AND status=1
              AND (@Query='' OR name LIKE CONCAT('%',@Query,'%')
                   OR municipality LIKE CONCAT('%',@Query,'%')
                   OR city LIKE CONCAT('%',@Query,'%'))
              AND (@PostalCode='' OR postal_code=@PostalCode)
            ORDER BY name,postal_code
            LIMIT 500
            """, new { StateId = stateId, Query = query, PostalCode = zipCode }));
    }
}
