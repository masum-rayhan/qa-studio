using System.Text.Json;
using AutoMapper;
using QAStudio.Application.Common.Exceptions;
using QAStudio.Application.TestCases.Interfaces;
using QAStudio.Application.TestRuns.DTOs;
using QAStudio.Application.TestRuns.Interfaces;
using QAStudio.Domain.Entities;
using QAStudio.Domain.Enums;
using QAStudio.Domain.Interfaces;

namespace QAStudio.Infrastructure.Services;

public class TestRunService : ITestRunService
{
    private readonly ITestRunRepository _testRunRepository;
    private readonly ITestCaseRepository _testCaseRepository;
    private readonly IEnvironmentRepository _environmentRepository;
    private readonly IMapper _mapper;

    public TestRunService(
        ITestRunRepository testRunRepository,
        ITestCaseRepository testCaseRepository,
        IEnvironmentRepository environmentRepository,
        IMapper mapper)
    {
        _testRunRepository = testRunRepository;
        _testCaseRepository = testCaseRepository;
        _environmentRepository = environmentRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<TestRunDto>> GetAllAsync(CancellationToken ct = default)
    {
        var runs = await _testRunRepository.GetAllAsync(ct);
        return _mapper.Map<List<TestRunDto>>(runs);
    }

    public async Task<IReadOnlyList<TestRunDto>> GetByTestCaseAsync(Guid testCaseId, CancellationToken ct = default)
    {
        var runs = await _testRunRepository.GetByTestCaseAsync(testCaseId, ct);
        return _mapper.Map<List<TestRunDto>>(runs);
    }

    public async Task<TestRunDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var run = await _testRunRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("TestRun", id);

        return _mapper.Map<TestRunDto>(run);
    }

    public async Task<TestRunDto> CreateAsync(CreateTestRunDto dto, Guid triggeredById, CancellationToken ct = default)
    {
        var testCase = await _testCaseRepository.GetByIdAsync(dto.TestCaseId, ct)
            ?? throw new NotFoundException("TestCase", dto.TestCaseId);

        if (!await _environmentRepository.ExistsAsync(dto.EnvironmentId, ct))
        {
            throw new BadRequestException($"Environment with ID '{dto.EnvironmentId}' not found.");
        }

        var run = new TestRun
        {
            TestCaseId = dto.TestCaseId,
            EnvironmentId = dto.EnvironmentId,
            TriggeredBy = triggeredById,
            Status = RunStatus.Pending
        };

        var created = await _testRunRepository.AddAsync(run, ct);
        return _mapper.Map<TestRunDto>(created);
    }

    public async Task<TestRunDto> ExecuteAsync(Guid runId, CancellationToken ct = default)
    {
        var run = await _testRunRepository.GetByIdAsync(runId, ct)
            ?? throw new NotFoundException("TestRun", runId);

        if (run.Status == RunStatus.Running)
        {
            throw new BadRequestException("Test run is already in progress.");
        }

        run.Status = RunStatus.Running;
        run.StartedAt = DateTime.UtcNow;
        await _testRunRepository.UpdateAsync(run, ct);

        // Playwright execution is implemented in Step 7-8
        // This stub returns the updated run with Pending status for now

        return _mapper.Map<TestRunDto>(run);
    }
}
