using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PsilmtyApi.Helpers;
using PsilmtyApi.Interfaces.Repositories;
using PsilmtyApi.Interfaces.Services;
using PsilmtyApi.Models.Requests;

namespace PsilmtyApi.Controllers;

[Authorize]
[ApiController]
[Route("api/calendario")]
public sealed class CalendarController(IApplicationDataService service, IDatabaseRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string type = "") =>
        Ok(await service.GetCalendarAsync(User.GetParishId(), type));

    [HttpPost]
    public async Task<IActionResult> Create(CalendarRequest request) =>
        Ok(await service.SaveCalendarAsync(null, User.GetParishId(), User.GetUserId(), request));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(uint id, CalendarRequest request) =>
        Ok(await service.SaveCalendarAsync(id, User.GetParishId(), User.GetUserId(), request));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(uint id)
    {
        await repository.ExecuteAsync(
            "UPDATE calendar SET status=0, updated_at=UTC_TIMESTAMP(), updated_by=@UserId WHERE id=@Id AND parish_id=@ParishId",
            new { Id = id, UserId = User.GetUserId(), ParishId = User.GetParishId() });
        return NoContent();
    }
}
