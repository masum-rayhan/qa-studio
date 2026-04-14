using QAStudio.Domain.Entities;

namespace QAStudio.Domain.Interfaces;

public interface IEnvironmentRepository
{
    Task<TargetEnvironment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<TargetEnvironment>> GetAllAsync(CancellationToken ct = default);
    Task<TargetEnvironment> AddAsync(TargetEnvironment environment, CancellationToken ct = default);
    Task UpdateAsync(TargetEnvironment environment, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken ct = default);
}
