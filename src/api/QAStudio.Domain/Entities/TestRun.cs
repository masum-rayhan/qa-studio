using QAStudio.Domain.Enums;

namespace QAStudio.Domain.Entities;

public class TestRun : BaseEntity
{
    public Guid TestCaseId { get; set; }
    public TestCase TestCase { get; set; } = null!;
    public Guid EnvironmentId { get; set; }
    public TargetEnvironment Environment { get; set; } = null!;
    public RunStatus Status { get; set; } = RunStatus.Pending;
    public Guid TriggeredBy { get; set; }
    public User TriggeredByUser { get; set; } = null!;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public long TotalDurationMs { get; set; }
    public string? VideoPath { get; set; }
    public string? TracePath { get; set; }

    // Navigation
    public ICollection<TestResult> Results { get; set; } = new List<TestResult>();
}
