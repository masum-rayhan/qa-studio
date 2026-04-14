using QAStudio.Domain.Entities;

namespace QAStudio.Domain.Interfaces;

public interface ITestRunRepository
{
    Task<TestRun?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<TestRun>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TestRun>> GetByTestCaseAsync(Guid testCaseId, CancellationToken ct = default);
    Task<TestRun> AddAsync(TestRun testRun, CancellationToken ct = default);
    Task UpdateAsync(TestRun testRun, CancellationToken ct = default);
    Task AddResultAsync(TestResult result, CancellationToken ct = default);
}
