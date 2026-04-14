namespace QAStudio.Application.Recording.DTOs;

public class CreateRecordingSessionDto
{
    public string Name { get; set; } = string.Empty;
    public string? TargetUrl { get; set; }
    public Guid? TargetEnvironmentId { get; set; }
}

public class RecordingSessionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? TargetUrl { get; set; }
    public string StepsJson { get; set; } = "[]";
    public string Status { get; set; } = string.Empty;
    public Guid CreatedBy { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public Guid? TargetEnvironmentId { get; set; }
    public string? TargetEnvironmentName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RecordingStepDto
{
    public string Action { get; set; } = string.Empty;
    public string? Selector { get; set; }
    public string? Value { get; set; }
    public string? Description { get; set; }
}

public class AddRecordingStepDto
{
    public string Action { get; set; } = string.Empty;
    public string? Selector { get; set; }
    public string? Value { get; set; }
    public string? Description { get; set; }
}

public class SaveRecordingDto
{
    public Guid TargetEnvironmentId { get; set; }
    public string? TagsJson { get; set; }
}
