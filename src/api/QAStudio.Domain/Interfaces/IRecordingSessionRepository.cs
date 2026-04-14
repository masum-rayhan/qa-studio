using QAStudio.Domain.Entities;

namespace QAStudio.Domain.Interfaces;

public interface IRecordingSessionRepository
{
    Task<RecordingSession?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<RecordingSession>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    Task<RecordingSession> AddAsync(RecordingSession session, CancellationToken ct = default);
    Task UpdateAsync(RecordingSession session, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
}
