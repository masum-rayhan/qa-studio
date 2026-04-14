using QAStudio.Application.TestRuns.DTOs;

namespace QAStudio.Application.TestRuns.Interfaces;

public interface ITestRunService
{
    Task<IReadOnlyList<TestRunDto>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TestRunDto>> GetByTestCaseAsync(Guid testCaseId, CancellationToken ct = default);
    Task<TestRunDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<TestRunDto> CreateAsync(CreateTestRunDto dto, Guid triggeredById, CancellationToken ct = default);
    Task<TestRunDto> ExecuteAsync(Guid runId, CancellationToken ct = default);
}
