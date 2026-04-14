using QAStudio.Domain.Entities;

namespace QAStudio.Domain.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default);
    Task<RefreshToken> AddAsync(RefreshToken refreshToken, CancellationToken ct = default);
    Task UpdateAsync(RefreshToken refreshToken, CancellationToken ct = default);
    Task RevokeAllUserTokensAsync(Guid userId, string? replacedByToken = null, CancellationToken ct = default);
}
