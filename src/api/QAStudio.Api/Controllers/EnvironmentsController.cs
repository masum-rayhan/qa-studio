using System.Security.Claims;
using QAStudio.Application.Common.Models;
using QAStudio.Application.Environments.DTOs;
using QAStudio.Application.Environments.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace QAStudio.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EnvironmentsController : ControllerBase
{
    private readonly IEnvironmentService _environmentService;

    public EnvironmentsController(IEnvironmentService environmentService)
    {
        _environmentService = environmentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var environments = await _environmentService.GetAllAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<EnvironmentDto>>.Ok(environments));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var environment = await _environmentService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<EnvironmentDto>.Ok(environment));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateEnvironmentDto dto, CancellationToken ct)
    {
        var userId = GetUserId();
        var environment = await _environmentService.CreateAsync(dto, userId, ct);
        return CreatedAtAction(nameof(GetById), new { id = environment.Id }, ApiResponse<EnvironmentDto>.Ok(environment));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEnvironmentDto dto, CancellationToken ct)
    {
        var environment = await _environmentService.UpdateAsync(id, dto, ct);
        return Ok(ApiResponse<EnvironmentDto>.Ok(environment));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _environmentService.DeleteAsync(id, ct);
        return Ok(ApiResponse<object>.Ok(new { message = "Environment deleted successfully." }));
    }

    private Guid GetUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token.");
        return Guid.Parse(userId);
    }
}
