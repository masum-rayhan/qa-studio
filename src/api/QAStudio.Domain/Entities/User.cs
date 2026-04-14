using QAStudio.Domain.Enums;

namespace QAStudio.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.QA;

    // Navigation
    public ICollection<TestCase> TestCases { get; set; } = new List<TestCase>();
    public ICollection<TestRun> TriggeredRuns { get; set; } = new List<TestRun>();
    public ICollection<TestSchedule> Schedules { get; set; } = new List<TestSchedule>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<TargetEnvironment> CreatedEnvironments { get; set; } = new List<TargetEnvironment>();
}
