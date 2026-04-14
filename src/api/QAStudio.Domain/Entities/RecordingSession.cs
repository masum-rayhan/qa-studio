namespace QAStudio.Domain.Entities;

public enum RecordingStatus
{
    Active = 0,
    Completed = 1,
    Cancelled = 2,
}

public class RecordingSession : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? TargetUrl { get; set; }
    public string StepsJson { get; set; } = "[]";
    public RecordingStatus Status { get; set; } = RecordingStatus.Active;
    public Guid CreatedBy { get; set; }
    public User Creator { get; set; } = null!;
    public Guid? TargetEnvironmentId { get; set; }
    public TargetEnvironment? TargetEnvironment { get; set; }
}
