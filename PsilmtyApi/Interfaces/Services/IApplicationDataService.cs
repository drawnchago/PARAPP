using PsilmtyApi.Models.Requests;

namespace PsilmtyApi.Interfaces.Services;

public interface IApplicationDataService
{
    Task<IReadOnlyList<dynamic>> GetNewsAsync(uint parishId, string query, DateTime? from = null, DateTime? to = null);
    Task<IReadOnlyList<dynamic>> GetAlexaNewsAsync(uint parishId, DateTime? from, DateTime? to);
    Task<IReadOnlyList<dynamic>> GetAlexaSchedulesAsync(uint parishId);
    Task<IReadOnlyList<dynamic>> GetAlexaCalendarAsync(uint parishId, string? type = null);
    Task<IReadOnlyList<dynamic>> GetAlexaMassesTodayAsync(uint parishId);
    Task<dynamic?> GetAlexaNextMassAsync(uint parishId);
    Task<dynamic?> GetAlexaParishContactAsync(uint parishId);
    Task<IReadOnlyList<dynamic>> GetAlexaAgendaAsync(uint parishId, DateOnly date);
    Task<dynamic> SaveNewsAsync(uint? id, uint parishId, uint userId, NewsRequest request);
    Task<IReadOnlyList<dynamic>> GetCalendarAsync(uint parishId, string type);
    Task<dynamic> SaveCalendarAsync(uint? id, uint parishId, uint userId, CalendarRequest request);
    Task<dynamic?> GetPreferencesAsync(uint userId);
    Task<dynamic> SavePreferencesAsync(uint userId, UserPreferencesRequest request);
}
