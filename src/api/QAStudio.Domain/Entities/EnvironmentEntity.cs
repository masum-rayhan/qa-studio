namespace QAStudio.Domain.Entities;

public class TargetEnvironment : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string? AuthToken { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid CreatedBy { get; set; }
    public User Creator { get; set; } = null!;

    // Navigation
    public ICollection<TestCase> TestCases { get; set; } = new List<TestCase>();
    public ICollection<TestRun> TestRuns { get; set; } = new List<TestRun>();
    public ICollection<TestSchedule> TestSchedules { get; set; } = new List<TestSchedule>();
    public ICollection<RecordingSession> RecordingSessions { get; set; } = new List<RecordingSession>();
}
