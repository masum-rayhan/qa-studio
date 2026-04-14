using QAStudio.Application.Recording.DTOs;
using QAStudio.Application.TestCases.DTOs;

namespace QAStudio.Application.Recording.Interfaces;

public interface IRecordingService
{
    Task<RecordingSessionDto> CreateSessionAsync(CreateRecordingSessionDto dto, Guid userId, CancellationToken ct = default);
    Task<RecordingSessionDto> GetSessionAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<RecordingSessionDto>> GetUserSessionsAsync(Guid userId, CancellationToken ct = default);
    Task<RecordingSessionDto> AddStepAsync(Guid sessionId, AddRecordingStepDto dto, CancellationToken ct = default);
    Task<RecordingSessionDto> CompleteSessionAsync(Guid sessionId, CancellationToken ct = default);
    Task DeleteSessionAsync(Guid id, CancellationToken ct = default);
    Task<TestCaseDto> SaveAsTestCaseAsync(Guid sessionId, SaveRecordingDto dto, Guid userId, CancellationToken ct = default);
}
