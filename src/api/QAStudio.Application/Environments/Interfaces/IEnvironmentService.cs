using QAStudio.Application.Environments.DTOs;

namespace QAStudio.Application.Environments.Interfaces;

public interface IEnvironmentService
{
    Task<IReadOnlyList<EnvironmentDto>> GetAllAsync(CancellationToken ct = default);
    Task<EnvironmentDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<EnvironmentDto> CreateAsync(CreateEnvironmentDto dto, Guid createdById, CancellationToken ct = default);
    Task<EnvironmentDto> UpdateAsync(Guid id, UpdateEnvironmentDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
