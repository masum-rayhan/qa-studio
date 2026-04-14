using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QAStudio.Application.Common.Models;
using QAStudio.Application.Recording.DTOs;
using QAStudio.Application.Recording.Interfaces;
using QAStudio.Application.TestCases.DTOs;

namespace QAStudio.Api.Controllers;

[ApiController]
[Route("api/recording")]
[Authorize]
public class RecordingController : ControllerBase
{
    private readonly IRecordingService _recordingService;
    private readonly ILogger<RecordingController> _logger;

    public RecordingController(
        IRecordingService recordingService,
        ILogger<RecordingController> logger)
    {
        _recordingService = recordingService;
        _logger = logger;
    }

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("start")]
    public async Task<IActionResult> StartSession(
        [FromBody] CreateRecordingSessionDto dto,
        CancellationToken ct)
    {
        var session = await _recordingService.CreateSessionAsync(dto, UserId, ct);
        return Ok(ApiResponse<RecordingSessionDto>.Ok(session));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetSession(Guid id, CancellationToken ct)
    {
        var session = await _recordingService.GetSessionAsync(id, ct);
        return Ok(ApiResponse<RecordingSessionDto>.Ok(session));
    }

    [HttpGet]
    public async Task<IActionResult> GetMySessions(CancellationToken ct)
    {
        var sessions = await _recordingService.GetUserSessionsAsync(UserId, ct);
        return Ok(ApiResponse<IReadOnlyList<RecordingSessionDto>>.Ok(sessions));
    }

    [HttpPost("{id:guid}/steps")]
    public async Task<IActionResult> AddStep(
        Guid id,
        [FromBody] AddRecordingStepDto dto,
        CancellationToken ct)
    {
        var session = await _recordingService.AddStepAsync(id, dto, ct);
        return Ok(ApiResponse<RecordingSessionDto>.Ok(session));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteSession(Guid id, CancellationToken ct)
    {
        await _recordingService.DeleteSessionAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> CompleteSession(Guid id, CancellationToken ct)
    {
        var session = await _recordingService.CompleteSessionAsync(id, ct);
        return Ok(ApiResponse<RecordingSessionDto>.Ok(session));
    }

    [HttpPost("{id:guid}/save")]
    public async Task<IActionResult> SaveAsTestCase(
        Guid id,
        [FromBody] SaveRecordingDto dto,
        CancellationToken ct)
    {
        var testCase = await _recordingService.SaveAsTestCaseAsync(id, dto, UserId, ct);
        return Ok(ApiResponse<TestCaseDto>.Ok(testCase));
    }

    [HttpGet("{id:guid}/events")]
    [Produces("text/event-stream")]
    public async Task Events(Guid id, CancellationToken ct)
    {
        Response.ContentType = "text/event-stream";
        Response.Headers["Cache-Control"] = "no-cache";
        Response.Headers["Connection"] = "keep-alive";

        var lastStepsCount = 0;

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var session = await _recordingService.GetSessionAsync(id, ct);
                var steps = JsonSerializer.Deserialize<List<RecordingStepDto>>(session.StepsJson) ?? [];

                if (steps.Count != lastStepsCount)
                {
                    var payload = JsonSerializer.Serialize(new
                    {
                        sessionId = session.Id,
                        stepsCount = steps.Count,
                        steps = steps,
                    });

                    await Response.WriteAsync($"data: {payload}\n\n", ct);
                    await Response.Body.FlushAsync(ct);
                    lastStepsCount = steps.Count;

                    if (session.Status != "Active")
                    {
                        await Response.WriteAsync($"event: closed\ndata: {{\"status\":\"{session.Status}\"}}\n\n", ct);
                        await Response.Body.FlushAsync(ct);
                        break;
                    }
                }

                await Task.Delay(500, ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SSE stream for session {SessionId}", id);
                break;
            }
        }
    }
}
