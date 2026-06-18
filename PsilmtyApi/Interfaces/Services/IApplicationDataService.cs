using PsilmtyApi.Models.Requests;

namespace PsilmtyApi.Interfaces.Services;

public interface IApplicationDataService
{
    Task<IReadOnlyList<dynamic>> GetNewsAsync(uint parishId, string query);
    Task<dynamic> SaveNewsAsync(uint? id, uint parishId, uint userId, NewsRequest request);
    Task<IReadOnlyList<dynamic>> GetCalendarAsync(uint parishId, string type);
    Task<dynamic> SaveCalendarAsync(uint? id, uint parishId, uint userId, CalendarRequest request);
    Task<dynamic?> GetPreferencesAsync(uint userId);
    Task<dynamic> SavePreferencesAsync(uint userId, UserPreferencesRequest request);
}
