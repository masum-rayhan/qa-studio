using QAStudio.Application.TestCases.DTOs;

namespace QAStudio.Application.TestCases.Interfaces;

public interface ITestCaseService
{
    Task<IReadOnlyList<TestCaseDto>> GetAllAsync(Guid? environmentId = null, CancellationToken ct = default);
    Task<TestCaseDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<TestCaseDto> CreateAsync(CreateTestCaseDto dto, Guid createdById, CancellationToken ct = default);
    Task<TestCaseDto> UpdateAsync(Guid id, UpdateTestCaseDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
