using QAStudio.Domain.Entities;

namespace QAStudio.Domain.Interfaces;

public interface ITestCaseRepository
{
    Task<TestCase?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<TestCase>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TestCase>> GetByEnvironmentAsync(Guid environmentId, CancellationToken ct = default);
    Task<TestCase> AddAsync(TestCase testCase, CancellationToken ct = default);
    Task UpdateAsync(TestCase testCase, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken ct = default);
}
