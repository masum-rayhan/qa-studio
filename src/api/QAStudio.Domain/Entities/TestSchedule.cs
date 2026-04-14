namespace QAStudio.Domain.Entities;

public class TestSchedule : BaseEntity
{
    public Guid TestCaseId { get; set; }
    public TestCase TestCase { get; set; } = null!;
    public Guid EnvironmentId { get; set; }
    public TargetEnvironment Environment { get; set; } = null!;
    public string CronExpression { get; set; } = "0 9 * * *";
    public bool IsActive { get; set; } = true;
    public DateTime? LastRunAt { get; set; }
    public DateTime? NextRunAt { get; set; }
    public Guid CreatedBy { get; set; }
    public User Creator { get; set; } = null!;
}
