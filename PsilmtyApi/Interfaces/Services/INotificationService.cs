using PsilmtyApi.Models.Requests;

namespace PsilmtyApi.Interfaces.Services;

public interface INotificationService
{
    Task<uint> ScheduleAsync(uint parishId, uint userId, NotificationRequest request);
    Task RegisterDeviceAsync(uint userId, DeviceRegistrationRequest request);
    Task<IReadOnlyList<dynamic>> GetUserNotificationsAsync(uint userId);
    Task MarkAsReadAsync(uint userId, uint notificationId);
    Task DispatchPendingAsync(CancellationToken cancellationToken);
}
