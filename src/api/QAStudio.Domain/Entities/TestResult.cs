using QAStudio.Domain.Enums;

namespace QAStudio.Domain.Entities;

public class TestResult : BaseEntity
{
    public Guid TestRunId { get; set; }
    public TestRun TestRun { get; set; } = null!;
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
