using System.Security.Claims;
using QAStudio.Application.Common.Models;
using QAStudio.Application.TestCases.DTOs;
using QAStudio.Application.TestCases.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace QAStudio.Api.Controllers;

[ApiController]
[Route("api/testcases")]
[Authorize]
public class TestCasesController : ControllerBase
{
    private readonly ITestCaseService _testCaseService;

    public TestCasesController(ITestCaseService testCaseService)
    {
        _testCaseService = testCaseService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? environmentId, CancellationToken ct)
    {
        var testCases = await _testCaseService.GetAllAsync(environmentId, ct);
        return Ok(ApiResponse<IReadOnlyList<TestCaseDto>>.Ok(testCases));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var testCase = await _testCaseService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<TestCaseDto>.Ok(testCase));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTestCaseDto dto, CancellationToken ct)
    {
        var userId = GetUserId();
        var testCase = await _testCaseService.CreateAsync(dto, userId, ct);
        return CreatedAtAction(nameof(GetById), new { id = testCase.Id }, ApiResponse<TestCaseDto>.Ok(testCase));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTestCaseDto dto, CancellationToken ct)
    {
        var testCase = await _testCaseService.UpdateAsync(id, dto, ct);
        return Ok(ApiResponse<TestCaseDto>.Ok(testCase));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _testCaseService.DeleteAsync(id, ct);
        return Ok(ApiResponse<object>.Ok(new { message = "Test case deleted successfully." }));
    }

    private Guid GetUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token.");
        return Guid.Parse(userId);
    }
}
