using QAStudio.Domain.Enums;

namespace QAStudio.Application.TestCases.DTOs;

public class TestCaseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string StepsJson { get; set; } = "[]";
    public string? TagsJson { get; set; }
    public Guid TargetEnvironmentId { get; set; }
    public string TargetEnvironmentName { get; set; } = string.Empty;
    public Guid CreatedBy { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
