using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PsilmtyApi.Helpers;
using PsilmtyApi.Interfaces.Repositories;
using PsilmtyApi.Interfaces.Services;
using PsilmtyApi.Models.Requests;

namespace PsilmtyApi.Controllers;

[Authorize]
[ApiController]
[Route("api/noticias")]
public sealed class NewsController(IApplicationDataService service, IDatabaseRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string q = "") =>
        Ok(await service.GetNewsAsync(User.GetParishId(), q));

    [HttpGet("categorias")]
    public async Task<IActionResult> GetCategories() =>
        Ok(await repository.QueryAsync<object>(
            "SELECT id Id, name Name FROM news_categories WHERE is_active=1 ORDER BY name"));

    [HttpPost]
    public async Task<IActionResult> Create(NewsRequest request) =>
        Ok(await service.SaveNewsAsync(null, User.GetParishId(), User.GetUserId(), request));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(uint id, NewsRequest request) =>
        Ok(await service.SaveNewsAsync(id, User.GetParishId(), User.GetUserId(), request));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(uint id)
    {
        await repository.ExecuteAsync(
            "UPDATE news SET is_active=0, updated_at=UTC_TIMESTAMP(), updated_by=@UserId WHERE id=@Id AND parish_id=@ParishId",
            new { Id = id, UserId = User.GetUserId(), ParishId = User.GetParishId() });
        return NoContent();
    }
}
