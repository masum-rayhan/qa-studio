using System.ComponentModel.DataAnnotations;

namespace QAStudio.Application.TestRuns.DTOs;

public class CreateTestRunDto
{
    [Required]
    public Guid TestCaseId { get; set; }

    [Required]
    public Guid EnvironmentId { get; set; }
}
