using Microsoft.EntityFrameworkCore;
using QAStudio.Domain.Entities;
using QAStudio.Domain.Interfaces;
using QAStudio.Infrastructure.Data;

namespace QAStudio.Infrastructure.Repositories;

public class RecordingSessionRepository : IRecordingSessionRepository
{
    private readonly AppDbContext _db;

    public RecordingSessionRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<RecordingSession?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.RecordingSessions
            .Include(rs => rs.Creator)
            .Include(rs => rs.TargetEnvironment)
            .FirstOrDefaultAsync(rs => rs.Id == id, ct);
    }

    public async Task<IReadOnlyList<RecordingSession>> GetByUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.RecordingSessions
            .Where(rs => rs.CreatedBy == userId)
            .Include(rs => rs.TargetEnvironment)
            .OrderByDescending(rs => rs.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<RecordingSession> AddAsync(RecordingSession session, CancellationToken ct = default)
    {
        _db.RecordingSessions.Add(session);
        await _db.SaveChangesAsync(ct);
        return session;
    }

    public async Task UpdateAsync(RecordingSession session, CancellationToken ct = default)
    {
        _db.RecordingSessions.Update(session);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var session = await _db.RecordingSessions.FindAsync([id], ct);
        if (session != null)
        {
            _db.RecordingSessions.Remove(session);
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.RecordingSessions.AnyAsync(rs => rs.Id == id, ct);
    }
}
