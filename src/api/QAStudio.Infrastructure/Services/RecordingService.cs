using System.Text.Json;
using AutoMapper;
using QAStudio.Application.Common.Exceptions;
using QAStudio.Application.Recording.DTOs;
using QAStudio.Application.Recording.Interfaces;
using QAStudio.Application.TestCases.DTOs;
using QAStudio.Application.TestCases.Interfaces;
using QAStudio.Domain.Entities;
using QAStudio.Domain.Interfaces;

namespace QAStudio.Infrastructure.Services;

public class RecordingService : IRecordingService
{
    private readonly IRecordingSessionRepository _sessionRepository;
    private readonly IEnvironmentRepository _environmentRepository;
    private readonly ITestCaseService _testCaseService;
    private readonly IMapper _mapper;

    public RecordingService(
        IRecordingSessionRepository sessionRepository,
        IEnvironmentRepository environmentRepository,
        ITestCaseService testCaseService,
        IMapper mapper)
    {
        _sessionRepository = sessionRepository;
        _environmentRepository = environmentRepository;
        _testCaseService = testCaseService;
        _mapper = mapper;
    }

    public async Task<RecordingSessionDto> CreateSessionAsync(
        CreateRecordingSessionDto dto,
        Guid userId,
        CancellationToken ct = default)
    {
        if (dto.TargetEnvironmentId.HasValue &&
            !await _environmentRepository.ExistsAsync(dto.TargetEnvironmentId.Value, ct))
        {
            throw new BadRequestException($"Environment with ID '{dto.TargetEnvironmentId}' not found.");
        }

        var session = new RecordingSession
        {
            Name = dto.Name,
            TargetUrl = dto.TargetUrl,
            TargetEnvironmentId = dto.TargetEnvironmentId,
            Status = RecordingStatus.Active,
            CreatedBy = userId,
        };

        var created = await _sessionRepository.AddAsync(session, ct);
        return _mapper.Map<RecordingSessionDto>(created);
    }

    public async Task<RecordingSessionDto> GetSessionAsync(Guid id, CancellationToken ct = default)
    {
        var session = await _sessionRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("RecordingSession", id);

        return _mapper.Map<RecordingSessionDto>(session);
    }

    public async Task<IReadOnlyList<RecordingSessionDto>> GetUserSessionsAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        var sessions = await _sessionRepository.GetByUserAsync(userId, ct);
        return _mapper.Map<List<RecordingSessionDto>>(sessions);
    }

    public async Task<RecordingSessionDto> AddStepAsync(
        Guid sessionId,
        AddRecordingStepDto dto,
        CancellationToken ct = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, ct)
            ?? throw new NotFoundException("RecordingSession", sessionId);

        if (session.Status != RecordingStatus.Active)
        {
            throw new BadRequestException("Recording session is no longer active.");
        }

        var steps = ParseSteps(session.StepsJson);
        steps.Add(new RecordingStepDto
        {
            Action = dto.Action,
            Selector = dto.Selector,
            Value = dto.Value,
            Description = dto.Description,
        });

        session.StepsJson = JsonSerializer.Serialize(steps);
        await _sessionRepository.UpdateAsync(session, ct);

        return _mapper.Map<RecordingSessionDto>(session);
    }

    public async Task<RecordingSessionDto> CompleteSessionAsync(
        Guid sessionId,
        CancellationToken ct = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, ct)
            ?? throw new NotFoundException("RecordingSession", sessionId);

        session.Status = RecordingStatus.Completed;
        await _sessionRepository.UpdateAsync(session, ct);

        return _mapper.Map<RecordingSessionDto>(session);
    }

    public async Task DeleteSessionAsync(Guid id, CancellationToken ct = default)
    {
        if (!await _sessionRepository.ExistsAsync(id, ct))
        {
            throw new NotFoundException("RecordingSession", id);
        }

        await _sessionRepository.DeleteAsync(id, ct);
    }

    public async Task<TestCaseDto> SaveAsTestCaseAsync(
        Guid sessionId,
        SaveRecordingDto dto,
        Guid userId,
        CancellationToken ct = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, ct)
            ?? throw new NotFoundException("RecordingSession", sessionId);

        if (session.Status != RecordingStatus.Active && session.Status != RecordingStatus.Completed)
        {
            throw new BadRequestException("Cannot save a cancelled recording session.");
        }

        if (!await _environmentRepository.ExistsAsync(dto.TargetEnvironmentId, ct))
        {
            throw new BadRequestException($"Environment with ID '{dto.TargetEnvironmentId}' not found.");
        }

        var createDto = new CreateTestCaseDto
        {
            Name = session.Name,
            Description = $"Recorded on {session.CreatedAt:yyyy-MM-dd}",
            StepsJson = session.StepsJson,
            TagsJson = dto.TagsJson,
            TargetEnvironmentId = dto.TargetEnvironmentId,
        };

        var testCase = await _testCaseService.CreateAsync(createDto, userId, ct);

        session.Status = RecordingStatus.Completed;
        await _sessionRepository.UpdateAsync(session, ct);

        return testCase;
    }

    private static List<RecordingStepDto> ParseSteps(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<RecordingStepDto>>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }
}
