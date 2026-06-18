using PsilmtyApi.Models.Requests;
using PsilmtyApi.Models.Responses;

namespace PsilmtyApi.Interfaces.Services;

public interface IParishScheduleService
{
    Task<IReadOnlyList<ParishScheduleDayResponse>> GetAsync(uint parishId);
    Task<IReadOnlyList<ParishScheduleDayResponse>> ReplaceAsync(
        uint parishId,
        uint userId,
        ParishScheduleRequest request,
        CancellationToken cancellationToken);
}
