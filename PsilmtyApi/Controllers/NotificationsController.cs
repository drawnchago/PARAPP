using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PsilmtyApi.Dictionaries;
using PsilmtyApi.Helpers;
using PsilmtyApi.Interfaces.Services;
using PsilmtyApi.Models.Requests;

namespace PsilmtyApi.Controllers;

[Authorize]
[ApiController]
[Route("api/notificaciones")]
public sealed class NotificationsController(INotificationService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get() =>
        Ok(await service.GetUserNotificationsAsync(User.GetUserId()));

    [HttpPost]
    public async Task<IActionResult> Schedule(NotificationRequest request)
    {
        if (request.Audience == NotificationDictionary.AllAudience && !User.IsSuperAdmin())
            return Forbid();

        var id = await service.ScheduleAsync(User.GetParishId(), User.GetUserId(), request);
        return Ok(new { id });
    }

    [HttpPut("{id:int}/leer")]
    public async Task<IActionResult> MarkAsRead(uint id)
    {
        await service.MarkAsReadAsync(User.GetUserId(), id);
        return NoContent();
    }

    [HttpPost("dispositivos")]
    public async Task<IActionResult> RegisterDevice(DeviceRegistrationRequest request)
    {
        await service.RegisterDeviceAsync(User.GetUserId(), request);
        return NoContent();
    }
}
