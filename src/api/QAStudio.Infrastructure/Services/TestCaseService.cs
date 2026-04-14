using System.Text.Json;
using AutoMapper;
using QAStudio.Application.Common.Exceptions;
using QAStudio.Application.TestCases.DTOs;
using QAStudio.Application.TestCases.Interfaces;
using QAStudio.Domain.Entities;
using QAStudio.Domain.Interfaces;

namespace QAStudio.Infrastructure.Services;

public class TestCaseService : ITestCaseService
{
    private readonly ITestCaseRepository _testCaseRepository;
    private readonly IEnvironmentRepository _environmentRepository;
    private readonly IMapper _mapper;

    public TestCaseService(
        ITestCaseRepository testCaseRepository,
        IEnvironmentRepository environmentRepository,
        IMapper mapper)
    {
        _testCaseRepository = testCaseRepository;
        _environmentRepository = environmentRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<TestCaseDto>> GetAllAsync(Guid? environmentId = null, CancellationToken ct = default)
    {
        var testCases = environmentId.HasValue
            ? await _testCaseRepository.GetByEnvironmentAsync(environmentId.Value, ct)
            : await _testCaseRepository.GetAllAsync(ct);

        return _mapper.Map<List<TestCaseDto>>(testCases);
    }

    public async Task<TestCaseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var testCase = await _testCaseRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("TestCase", id);

        return _mapper.Map<TestCaseDto>(testCase);
    }

    public async Task<TestCaseDto> CreateAsync(CreateTestCaseDto dto, Guid createdById, CancellationToken ct = default)
    {
        if (!await _environmentRepository.ExistsAsync(dto.TargetEnvironmentId, ct))
        {
            throw new BadRequestException($"Environment with ID '{dto.TargetEnvironmentId}' not found.");
        }

        if (!IsValidStepsJson(dto.StepsJson))
        {
            throw new BadRequestException("StepsJson is not a valid JSON array.");
        }

        var testCase = _mapper.Map<TestCase>(dto);
        testCase.CreatedBy = createdById;

        var created = await _testCaseRepository.AddAsync(testCase, ct);
        return _mapper.Map<TestCaseDto>(created);
    }

    public async Task<TestCaseDto> UpdateAsync(Guid id, UpdateTestCaseDto dto, CancellationToken ct = default)
    {
        var testCase = await _testCaseRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("TestCase", id);

        if (!await _environmentRepository.ExistsAsync(dto.TargetEnvironmentId, ct))
        {
            throw new BadRequestException($"Environment with ID '{dto.TargetEnvironmentId}' not found.");
        }

        if (!IsValidStepsJson(dto.StepsJson))
        {
            throw new BadRequestException("StepsJson is not a valid JSON array.");
        }

        _mapper.Map(dto, testCase);
        await _testCaseRepository.UpdateAsync(testCase, ct);
        return _mapper.Map<TestCaseDto>(testCase);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        if (!await _testCaseRepository.ExistsAsync(id, ct))
        {
            throw new NotFoundException("TestCase", id);
        }

        await _testCaseRepository.DeleteAsync(id, ct);
    }

    private static bool IsValidStepsJson(string json)
    {
        try
        {
            var parsed = JsonDocument.Parse(json);
            return parsed.RootElement.ValueKind == JsonValueKind.Array;
        }
        catch
        {
            return false;
        }
    }
}
