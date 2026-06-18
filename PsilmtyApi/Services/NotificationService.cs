using PsilmtyApi.Interfaces.Repositories;
using PsilmtyApi.Interfaces.Services;
using PsilmtyApi.Models.Requests;

namespace PsilmtyApi.Services;

public sealed class NotificationService(
    IDatabaseRepository repository,
    IPushNotificationSender pushSender) : INotificationService
{
    public Task<uint> ScheduleAsync(uint parishId, uint userId, NotificationRequest request) =>
        repository.ExecuteScalarAsync<uint>("""
            INSERT INTO notifications
                (parish_id, created_by_user, title, body, image_url, type,
                 reference_type, reference_id, audience, audience_role_id,
                 audience_user_id, scheduled_at, is_sent, is_active, created_at, created_by)
            VALUES
                (@ParishId, @UserId, @Title, @Body, @ImageUrl, @Type,
                 @ReferenceType, @ReferenceId, @Audience, @AudienceRoleId,
                 @AudienceUserId, @ScheduledAt, 0, 1, UTC_TIMESTAMP(), @UserId);
            SELECT LAST_INSERT_ID();
            """, new
        {
            ParishId = parishId,
            UserId = userId,
            request.Title,
            request.Body,
            request.ImageUrl,
            request.Type,
            request.ReferenceType,
            request.ReferenceId,
            request.Audience,
            request.AudienceRoleId,
            request.AudienceUserId,
            ScheduledAt = request.ScheduledAt ?? DateTime.UtcNow
        });

    public Task RegisterDeviceAsync(uint userId, DeviceRegistrationRequest request) =>
        repository.ExecuteAsync("""
            INSERT INTO user_devices
                (user_id, push_token, platform, device_model, os_version, app_version,
                 last_used_at, is_active, created_at, created_by)
            VALUES
                (@UserId, @PushToken, @Platform, @DeviceModel, @OsVersion, @AppVersion,
                 UTC_TIMESTAMP(), 1, UTC_TIMESTAMP(), @UserId)
            ON DUPLICATE KEY UPDATE
                user_id=VALUES(user_id), platform=VALUES(platform),
                device_model=VALUES(device_model), os_version=VALUES(os_version),
                app_version=VALUES(app_version), last_used_at=UTC_TIMESTAMP(), is_active=1,
                updated_at=UTC_TIMESTAMP(), updated_by=@UserId
            """, new
        {
            UserId = userId,
            request.PushToken,
            request.Platform,
            request.DeviceModel,
            request.OsVersion,
            request.AppVersion
        });

    public Task<IReadOnlyList<dynamic>> GetUserNotificationsAsync(uint userId) =>
        repository.QueryAsync<dynamic>("""
            SELECT n.id Id, n.title Title, n.body Body, n.image_url ImageUrl, n.type Type,
                   n.reference_type ReferenceType, n.reference_id ReferenceId,
                   nu.is_read IsRead, nu.read_at ReadAt, nu.received_at ReceivedAt
            FROM notification_users nu
            JOIN notifications n ON n.id = nu.notification_id
            WHERE nu.user_id=@UserId AND n.is_active=1
            ORDER BY nu.received_at DESC
            """, new { UserId = userId });

    public Task MarkAsReadAsync(uint userId, uint notificationId) =>
        repository.ExecuteAsync("""
            UPDATE notification_users SET is_read=1, read_at=UTC_TIMESTAMP(),
                updated_at=UTC_TIMESTAMP(), updated_by=@UserId
            WHERE notification_id=@NotificationId AND user_id=@UserId
            """, new { UserId = userId, NotificationId = notificationId });

    public async Task DispatchPendingAsync(CancellationToken cancellationToken)
    {
        var pending = await repository.QueryAsync<PendingNotification>("""
            SELECT id, parish_id ParishId, title, body, image_url ImageUrl, type,
                   audience, audience_role_id AudienceRoleId, audience_user_id AudienceUserId
            FROM notifications
            WHERE is_active=1 AND is_sent=0 AND COALESCE(scheduled_at, created_at) <= UTC_TIMESTAMP()
            ORDER BY COALESCE(scheduled_at, created_at)
            LIMIT 100
            """);

        foreach (var notification in pending)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var recipients = await repository.QueryAsync<DeviceRecipient>("""
                SELECT DISTINCT u.id UserId, d.push_token PushToken
                FROM users u
                JOIN user_preferences pref ON pref.user_id=u.id AND pref.push_notifications=1
                JOIN user_devices d ON d.user_id=u.id AND d.is_active=1
                LEFT JOIN user_roles ur ON ur.user_id=u.id
                WHERE u.is_active=1 AND u.parish_id=@ParishId
                  AND (@Audience='all'
                    OR (@Audience='user' AND u.id=@AudienceUserId)
                    OR (@Audience='role' AND ur.role_id=@AudienceRoleId))
                  AND (@Type NOT IN ('event','reminder') OR pref.notify_events=1)
                  AND (@Type <> 'news' OR pref.notify_news=1)
                  AND (@Type <> 'reminder' OR pref.notify_reminders=1)
                """, notification);

            if (recipients.Count == 0) continue;
            var sent = await pushSender.SendAsync(
                notification.Title,
                notification.Body,
                notification.ImageUrl,
                recipients.Select(item => item.PushToken).Distinct().ToArray(),
                cancellationToken);

            if (sent <= 0) continue;

            foreach (var userId in recipients.Select(item => item.UserId).Distinct())
            {
                await repository.ExecuteAsync("""
                    INSERT IGNORE INTO notification_users
                        (notification_id, user_id, received_at, created_at, created_by)
                    VALUES (@NotificationId, @UserId, UTC_TIMESTAMP(), UTC_TIMESTAMP(), @UserId)
                    """, new { NotificationId = notification.Id, UserId = userId });
            }

            await repository.ExecuteAsync("""
                UPDATE notifications SET is_sent=1, sent_at=UTC_TIMESTAMP(), total_sent=@Sent,
                    updated_at=UTC_TIMESTAMP()
                WHERE id=@Id
                """, new { notification.Id, Sent = sent });
        }
    }

    private sealed class PendingNotification
    {
        public uint Id { get; set; }
        public uint ParishId { get; set; }
        public string Title { get; set; } = "";
        public string Body { get; set; } = "";
        public string? ImageUrl { get; set; }
        public string Type { get; set; } = "";
        public string Audience { get; set; } = "";
        public uint? AudienceRoleId { get; set; }
        public uint? AudienceUserId { get; set; }
    }

    private sealed class DeviceRecipient
    {
        public uint UserId { get; set; }
        public string PushToken { get; set; } = "";
    }
}
