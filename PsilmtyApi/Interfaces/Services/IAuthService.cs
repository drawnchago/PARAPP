using PsilmtyApi.Models.Requests;
using PsilmtyApi.Models.Responses;

namespace PsilmtyApi.Interfaces.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<LoginResponse> RegisterAsync(RegisterRequest request);
}
