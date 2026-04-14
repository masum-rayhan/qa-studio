using QAStudio.Domain.Enums;

namespace QAStudio.Application.TestRuns.DTOs;

public class TestRunDto
{
    public Guid Id { get; set; }
    public Guid TestCaseId { get; set; }
    public string TestCaseName { get; set; } = string.Empty;
    public Guid EnvironmentId { get; set; }
    public string EnvironmentName { get; set; } = string.Empty;
    public RunStatus Status { get; set; }
    public Guid TriggeredBy { get; set; }
    public string TriggeredByName { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public long TotalDurationMs { get; set; }
    public string? VideoPath { get; set; }
    public string? TracePath { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<TestResultDto> Results { get; set; } = new();
}

public class TestResultDto
{
    public Guid Id { get; set; }
    public int StepIndex { get; set; }
    public StepAction Action { get; set; }
    public string? Selector { get; set; }
    public string? Value { get; set; }
    public string? Description { get; set; }
    public ResultStatus Status { get; set; }
    public long DurationMs { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ScreenshotPath { get; set; }
}
