namespace QAStudio.Domain.Entities;

public class TestCase : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string StepsJson { get; set; } = "[]";
    public string? TagsJson { get; set; }
    public Guid TargetEnvironmentId { get; set; }
    public TargetEnvironment TargetEnvironment { get; set; } = null!;
    public Guid CreatedBy { get; set; }
    public User Creator { get; set; } = null!;

    // Navigation
    public ICollection<TestRun> TestRuns { get; set; } = new List<TestRun>();
    public ICollection<TestSchedule> TestSchedules { get; set; } = new List<TestSchedule>();
}
