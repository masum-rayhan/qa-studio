using QAStudio.Application.Auth.DTOs;
using QAStudio.Domain.Entities;

namespace QAStudio.Application.Auth.Interfaces;

public interface IAuthService
{
    Task<User> RegisterAsync(RegisterDto dto, CancellationToken ct = default);
    Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken ct = default);
    Task<TokenResponseDto> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task<UserProfileDto> GetProfileAsync(Guid userId, CancellationToken ct = default);
}
