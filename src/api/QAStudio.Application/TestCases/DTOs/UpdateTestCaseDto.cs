using System.ComponentModel.DataAnnotations;

namespace QAStudio.Application.TestCases.DTOs;

public class UpdateTestCaseDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public string StepsJson { get; set; } = "[]";

    public string? TagsJson { get; set; }

    [Required]
    public Guid TargetEnvironmentId { get; set; }
}
