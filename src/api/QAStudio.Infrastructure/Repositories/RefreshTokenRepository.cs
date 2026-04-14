using Microsoft.EntityFrameworkCore;
using QAStudio.Domain.Entities;
using QAStudio.Domain.Interfaces;
using QAStudio.Infrastructure.Data;

namespace QAStudio.Infrastructure.Repositories;

public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(AppDbContext context) : base(context) { }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default)
        => await _dbSet.FirstOrDefaultAsync(rt => rt.Token == token, ct);

    public async Task RevokeAllUserTokensAsync(Guid userId, string? replacedByToken = null, CancellationToken ct = default)
    {
        var tokens = await _dbSet.Where(rt => rt.UserId == userId && rt.RevokedAt == null).ToListAsync(ct);
        foreach (var token in tokens)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.ReplacedByToken = replacedByToken;
        }
        await _context.SaveChangesAsync(ct);
    }
}
