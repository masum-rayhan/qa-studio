using System.Security.Claims;
using QAStudio.Application.Common.Models;
using QAStudio.Application.TestRuns.DTOs;
using QAStudio.Application.TestRuns.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace QAStudio.Api.Controllers;

[ApiController]
[Route("api/runs")]
[Authorize]
public class TestRunsController : ControllerBase
{
    private readonly ITestRunService _testRunService;

    public TestRunsController(ITestRunService testRunService)
    {
        _testRunService = testRunService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var runs = await _testRunService.GetAllAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<TestRunDto>>.Ok(runs));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var run = await _testRunService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<TestRunDto>.Ok(run));
    }

    [HttpGet("testcase/{testCaseId:guid}")]
    public async Task<IActionResult> GetByTestCase(Guid testCaseId, CancellationToken ct)
    {
        var runs = await _testRunService.GetByTestCaseAsync(testCaseId, ct);
        return Ok(ApiResponse<IReadOnlyList<TestRunDto>>.Ok(runs));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTestRunDto dto, CancellationToken ct)
    {
        var userId = GetUserId();
        var run = await _testRunService.CreateAsync(dto, userId, ct);
        return CreatedAtAction(nameof(GetById), new { id = run.Id }, ApiResponse<TestRunDto>.Ok(run));
    }

    [HttpPost("{id:guid}/execute")]
    public async Task<IActionResult> Execute(Guid id, CancellationToken ct)
    {
        var run = await _testRunService.ExecuteAsync(id, ct);
        return Ok(ApiResponse<TestRunDto>.Ok(run));
    }

    private Guid GetUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token.");
        return Guid.Parse(userId);
    }
}
