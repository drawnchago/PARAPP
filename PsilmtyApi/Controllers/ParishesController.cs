using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PsilmtyApi.Helpers;
using PsilmtyApi.Interfaces.Repositories;
using PsilmtyApi.Interfaces.Services;
using PsilmtyApi.Models.Requests;

namespace PsilmtyApi.Controllers;

[Authorize]
[ApiController]
[Route("api/parroquias")]
public sealed class ParishesController(
    IDatabaseRepository repository,
    IParishScheduleService scheduleService) : ControllerBase
{
    [HttpGet("{id:int}/horarios")]
    public async Task<IActionResult> GetSchedules(uint id)
    {
        if (!User.IsSuperAdmin() && id != User.GetParishId()) return Forbid();
        return Ok(await scheduleService.GetAsync(id));
    }

    [HttpPut("{id:int}/horarios")]
    public async Task<IActionResult> SaveSchedules(
        uint id,
        ParishScheduleRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.IsSuperAdmin() && id != User.GetParishId()) return Forbid();
        return Ok(await scheduleService.ReplaceAsync(id, User.GetUserId(), request, cancellationToken));
    }

    [HttpPost("logo")]
    [RequestSizeLimit(2 * 1024 * 1024)]
    public async Task<IActionResult> UploadLogo(IFormFile logo, [FromForm] uint? parishId, [FromServices] IWebHostEnvironment environment)
    {
        if (logo.Length == 0) return BadRequest(new { message = "Logo file is required." });
        var targetParishId = User.IsSuperAdmin() && parishId.HasValue ? parishId.Value : User.GetParishId();
        var extension = Path.GetExtension(logo.FileName).ToLowerInvariant();
        if (extension is not ".png" and not ".jpg" and not ".jpeg" and not ".webp" and not ".svg")
            return BadRequest(new { message = "Unsupported image format." });

        var directory = Path.Combine(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"), "uploads", "parishes");
        Directory.CreateDirectory(directory);
        var fileName = $"parish-{targetParishId}-{Guid.NewGuid():N}{extension}";
        await using (var stream = System.IO.File.Create(Path.Combine(directory, fileName)))
            await logo.CopyToAsync(stream);

        var logoUrl = $"/uploads/parishes/{fileName}";
        await repository.ExecuteAsync(
            "UPDATE parishes SET logo_url=@LogoUrl,updated_at=UTC_TIMESTAMP(),updated_by=@UserId WHERE id=@Id",
            new { LogoUrl = logoUrl, UserId = User.GetUserId(), Id = targetParishId });
        return Ok(new { logoUrl });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        if (!User.IsSuperAdmin()) return Forbid();
        return Ok(await GetParishesAsync(null));
    }

    [HttpGet("mia")]
    public async Task<IActionResult> GetMine() =>
        Ok((await GetParishesAsync(User.GetParishId())).SingleOrDefault());

    [HttpPut("mia")]
    public async Task<IActionResult> UpdateMine(Dictionary<string, object?> request) =>
        await UpdateInternal(User.GetParishId(), request);

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(uint id, Dictionary<string, object?> request)
    {
        if (!User.IsSuperAdmin()) return Forbid();
        return await UpdateInternal(id, request);
    }

    private async Task<IActionResult> UpdateInternal(uint id, Dictionary<string, object?> request)
    {
        var stateId = GetUInt(request.GetValueOrDefault("stateId"));
        var neighborhoodId = GetUInt(request.GetValueOrDefault("neighborhoodId"));
        if (neighborhoodId.HasValue)
        {
            if (!stateId.HasValue || await repository.ExecuteScalarAsync<int>("""
                SELECT COUNT(*) FROM neighborhoods
                WHERE id=@NeighborhoodId AND state_id=@StateId AND status=1
                """, new { StateId = stateId.Value, NeighborhoodId = neighborhoodId.Value }) != 1)
                return BadRequest(new { message = "The selected neighborhood does not belong to the selected state." });
        }

        var allowed = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["name"]="name", ["diocese"]="diocese", ["address"]="address", ["city"]="city",
            ["state"]="state", ["country"]="country", ["phone"]="phone", ["email"]="email",
            ["website"]="website", ["logoUrl"]="logo_url", ["imageUrl"]="image_url",
            ["description"]="description", ["patronSaint"]="patron_saint",
            ["stateId"]="state_id", ["neighborhoodId"]="neighborhood_id"
        };
        var updates = request.Where(item => allowed.ContainsKey(item.Key)).ToArray();
        if (updates.Length == 0) return BadRequest(new { message = "No supported fields were provided." });

        var parameters = new Dapper.DynamicParameters();
        parameters.Add("Id", id);
        parameters.Add("UserId", User.GetUserId());
        var assignments = new List<string>();
        for (var index = 0; index < updates.Length; index++)
        {
            var name = $"Value{index}";
            assignments.Add($"{allowed[updates[index].Key]}=@{name}");
            parameters.Add(name, updates[index].Key.Equals("stateId", StringComparison.OrdinalIgnoreCase)
                    ? stateId
                : updates[index].Key.Equals("neighborhoodId", StringComparison.OrdinalIgnoreCase)
                    ? neighborhoodId
                    : GetValue(updates[index].Value));
        }
        await repository.ExecuteAsync(
            $"UPDATE parishes SET {string.Join(",", assignments)},updated_at=UTC_TIMESTAMP(),updated_by=@UserId WHERE id=@Id",
            parameters);
        await repository.ExecuteAsync("""
            UPDATE parishes p
            LEFT JOIN states s ON s.id=p.state_id
            LEFT JOIN neighborhoods n ON n.id=p.neighborhood_id
            SET p.state=COALESCE(s.name,p.state),
                p.city=COALESCE(n.city,p.city),
                p.updated_at=UTC_TIMESTAMP(),p.updated_by=@UserId
            WHERE p.id=@Id
            """, new { Id = id, UserId = User.GetUserId() });
        return Ok((await GetParishesAsync(id)).SingleOrDefault());
    }

    private Task<IReadOnlyList<object>> GetParishesAsync(uint? id) =>
        repository.QueryAsync<object>("""
            SELECT p.id Id,p.name Name,p.diocese Diocese,p.address Address,p.city City,
                   p.state State,p.country Country,p.phone Phone,p.email Email,p.website Website,
                   p.logo_url LogoUrl,p.image_url ImageUrl,p.description Description,
                   p.patron_saint PatronSaint,p.status IsActive,p.state_id StateId,
                   p.neighborhood_id NeighborhoodId,s.country_id CountryId,
                   n.name Neighborhood,n.postal_code ZipCode
            FROM parishes p
            LEFT JOIN states s ON s.id=p.state_id
            LEFT JOIN neighborhoods n ON n.id=p.neighborhood_id
            WHERE p.status=1 AND (@Id IS NULL OR p.id=@Id)
            ORDER BY p.name
            """, new { Id = id });

    private static uint? GetUInt(object? value)
    {
        if (value is null) return null;
        if (value is System.Text.Json.JsonElement element)
            return element.ValueKind == System.Text.Json.JsonValueKind.Number && element.TryGetUInt32(out var number)
                ? number
                : null;
        return uint.TryParse(value.ToString(), out var parsed) ? parsed : null;
    }

    private static object? GetValue(object? value)
    {
        if (value is not System.Text.Json.JsonElement element) return value;
        return element.ValueKind switch
        {
            System.Text.Json.JsonValueKind.String => element.GetString(),
            System.Text.Json.JsonValueKind.Number when element.TryGetInt64(out var number) => number,
            System.Text.Json.JsonValueKind.True => true,
            System.Text.Json.JsonValueKind.False => false,
            System.Text.Json.JsonValueKind.Null => null,
            _ => element.ToString()
        };
    }
}
