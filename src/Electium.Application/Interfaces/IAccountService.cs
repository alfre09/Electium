using Electium.Application.Dtos;
using Electium.Application.ViewModels;

namespace Electium.Application.Interfaces;

public interface IAccountService
{
    /// <summary>Devuelve null si el inicio de sesión fue correcto; si no, el mensaje de error.</summary>
    Task<string?> LoginAsync(LoginViewModel model);

    Task LogoutAsync();

    Task<List<LeaderDto>> GetLeadersAsync();

    Task CreateLeaderAsync(LeaderViewModel model);

    Task SetLeaderActiveAsync(string userId, bool isActive);

    Task DeleteLeaderAsync(string userId);
}
