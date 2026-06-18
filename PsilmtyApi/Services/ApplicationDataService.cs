using Dapper;
using PsilmtyApi.Interfaces.Repositories;
using PsilmtyApi.Interfaces.Services;
using PsilmtyApi.Models.Requests;

namespace PsilmtyApi.Services;

public sealed class ApplicationDataService(
    IDatabaseRepository repository,
    INotificationService notificationService) : IApplicationDataService
{
    public async Task<IReadOnlyList<dynamic>> GetNewsAsync(uint parishId, string query) =>
        await repository.QueryAsync<dynamic>("""
            SELECT n.id Id, n.title Title, n.summary Summary, n.content Content, n.image_url ImageUrl,
                   n.category_id CategoryId, c.name CategoryName,
                   CONCAT_WS(' ', u.first_name, u.last_name) AuthorName,
                   n.is_published IsPublished, n.published_at PublishedAt,
                   n.is_featured IsFeatured, n.views Views, n.created_at CreatedAt
            FROM news n
            LEFT JOIN news_categories c ON c.id = n.category_id
            LEFT JOIN users u ON u.id = n.author_id
            WHERE n.parish_id = @ParishId AND n.is_active = 1
              AND (@Query = '' OR n.title LIKE CONCAT('%', @Query, '%'))
            ORDER BY COALESCE(n.published_at, n.created_at) DESC
            """, new { ParishId = parishId, Query = query });

    public async Task<dynamic> SaveNewsAsync(uint? id, uint parishId, uint userId, NewsRequest request)
    {
        var publishedAt = request.PublishedAt ?? (request.IsPublished ? DateTime.UtcNow : null);
        uint newsId;

        if (id.HasValue)
        {
            await repository.ExecuteAsync("""
                UPDATE news SET category_id=@CategoryId, title=@Title, summary=@Summary, content=@Content,
                    image_url=@ImageUrl, is_published=@IsPublished, published_at=@PublishedAt,
                    is_featured=@IsFeatured, updated_at=UTC_TIMESTAMP(), updated_by=@UserId
                WHERE id=@Id AND parish_id=@ParishId
                """, new
            {
                Id = id.Value, ParishId = parishId, UserId = userId,
                request.CategoryId, request.Title, request.Summary, request.Content,
                request.ImageUrl, request.IsPublished, PublishedAt = publishedAt, request.IsFeatured
            });
            newsId = id.Value;
        }
        else
        {
            newsId = await repository.ExecuteScalarAsync<uint>("""
                INSERT INTO news
                    (parish_id, category_id, author_id, title, summary, content, image_url,
                     is_published, published_at, is_featured, is_active, created_at, created_by)
                VALUES
                    (@ParishId, @CategoryId, @UserId, @Title, @Summary, @Content, @ImageUrl,
                     @IsPublished, @PublishedAt, @IsFeatured, 1, UTC_TIMESTAMP(), @UserId);
                SELECT LAST_INSERT_ID();
                """, new
            {
                ParishId = parishId, UserId = userId, request.CategoryId, request.Title,
                request.Summary, request.Content, request.ImageUrl, request.IsPublished,
                PublishedAt = publishedAt, request.IsFeatured
            });
        }

        if (request.SendNotification)
        {
            var scheduledAt = publishedAt?.AddMinutes(-Math.Max(0, request.ReminderMinutes));
            await notificationService.ScheduleAsync(parishId, userId, new NotificationRequest
            {
                Title = request.Title,
                Body = request.Summary ?? request.Content,
                ImageUrl = request.ImageUrl,
                Type = "news",
                ReferenceType = "news",
                ReferenceId = newsId,
                ScheduledAt = scheduledAt,
                Audience = "all"
            });
        }

        return (await GetNewsAsync(parishId, "")).First(item => (uint)item.Id == newsId);
    }

    public async Task<IReadOnlyList<dynamic>> GetCalendarAsync(uint parishId, string type) =>
        await repository.QueryAsync<dynamic>("""
            SELECT id Id, title Title, description Description, color Color,
                   start_datetime StartDatetime, end_datetime EndDatetime, all_day AllDay,
                   type Type, location Location, reminder_minutes ReminderMinutes
            FROM calendar
            WHERE parish_id=@ParishId AND is_active=1 AND is_visible=1
              AND (@Type='' OR type=@Type)
            ORDER BY start_datetime
            """, new { ParishId = parishId, Type = type });

    public async Task<dynamic> SaveCalendarAsync(uint? id, uint parishId, uint userId, CalendarRequest request)
    {
        uint calendarId;
        if (id.HasValue)
        {
            await repository.ExecuteAsync("""
                UPDATE calendar SET title=@Title, description=@Description, color=@Color,
                    start_datetime=@StartDatetime, end_datetime=@EndDatetime, all_day=@AllDay,
                    type=@Type, location=@Location, reminder_minutes=@ReminderMinutes,
                    updated_at=UTC_TIMESTAMP(), updated_by=@UserId
                WHERE id=@Id AND parish_id=@ParishId
                """, new
            {
                Id = id.Value, ParishId = parishId, UserId = userId, request.Title,
                request.Description, request.Color, request.StartDatetime, request.EndDatetime,
                request.AllDay, request.Type, request.Location, request.ReminderMinutes
            });
            calendarId = id.Value;
        }
        else
        {
            calendarId = await repository.ExecuteScalarAsync<uint>("""
                INSERT INTO calendar
                    (parish_id, title, description, color, start_datetime, end_datetime,
                     all_day, type, location, reminder_minutes, is_visible, is_active, created_at, created_by)
                VALUES
                    (@ParishId, @Title, @Description, @Color, @StartDatetime, @EndDatetime,
                     @AllDay, @Type, @Location, @ReminderMinutes, 1, 1, UTC_TIMESTAMP(), @UserId);
                SELECT LAST_INSERT_ID();
                """, new
            {
                ParishId = parishId, UserId = userId, request.Title, request.Description,
                request.Color, request.StartDatetime, request.EndDatetime, request.AllDay,
                request.Type, request.Location, request.ReminderMinutes
            });
        }

        if (request.SendNotification)
        {
            await notificationService.ScheduleAsync(parishId, userId, new NotificationRequest
            {
                Title = request.Title,
                Body = request.Description ?? request.Title,
                Type = "event",
                ReferenceType = "calendar",
                ReferenceId = calendarId,
                ScheduledAt = request.StartDatetime.AddMinutes(-Math.Max(0, request.ReminderMinutes)),
                Audience = "all"
            });
        }

        return (await GetCalendarAsync(parishId, "")).First(item => (uint)item.Id == calendarId);
    }

    public Task<dynamic?> GetPreferencesAsync(uint userId) =>
        repository.QuerySingleAsync<dynamic>("""
            SELECT language_id LenguajeId, theme_id TemaId,
                   push_notifications NotifPushActivas, notify_events NotifEventos,
                   notify_news NotifNoticias, notify_reminders NotifRecordatorios,
                   notify_adoration NotifAdoracion, notify_sound NotifSonido,
                   notify_vibration NotifVibracion, calendar_view CalendarioVista,
                   calendar_first_day CalendarioPrimerDia, font_size TamanoFuente,
                   high_contrast AltoContraste
            FROM user_preferences WHERE user_id=@UserId
            """, new { UserId = userId });

    public async Task<dynamic> SavePreferencesAsync(uint userId, UserPreferencesRequest request)
    {
        var current = await GetPreferencesAsync(userId) ?? throw new KeyNotFoundException("User preferences were not found.");
        await repository.ExecuteAsync("""
            UPDATE user_preferences SET
                language_id=COALESCE(@LenguajeId, language_id),
                theme_id=COALESCE(@TemaId, theme_id),
                push_notifications=COALESCE(@NotifPushActivas, push_notifications),
                notify_events=COALESCE(@NotifEventos, notify_events),
                notify_news=COALESCE(@NotifNoticias, notify_news),
                notify_reminders=COALESCE(@NotifRecordatorios, notify_reminders),
                notify_adoration=COALESCE(@NotifAdoracion, notify_adoration),
                notify_sound=COALESCE(@NotifSonido, notify_sound),
                notify_vibration=COALESCE(@NotifVibracion, notify_vibration),
                calendar_view=COALESCE(@CalendarioVista, calendar_view),
                calendar_first_day=COALESCE(@CalendarioPrimerDia, calendar_first_day),
                font_size=COALESCE(@TamanoFuente, font_size),
                high_contrast=COALESCE(@AltoContraste, high_contrast),
                updated_at=UTC_TIMESTAMP(), updated_by=@UserId
            WHERE user_id=@UserId
            """, new
        {
            UserId = userId, request.LenguajeId, request.TemaId, request.NotifPushActivas,
            request.NotifEventos, request.NotifNoticias, request.NotifRecordatorios,
            request.NotifAdoracion, request.NotifSonido, request.NotifVibracion,
            request.CalendarioVista, request.CalendarioPrimerDia, request.TamanoFuente,
            request.AltoContraste
        });
        return (await GetPreferencesAsync(userId))!;
    }
}
