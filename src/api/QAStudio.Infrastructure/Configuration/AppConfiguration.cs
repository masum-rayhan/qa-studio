namespace QAStudio.Infrastructure.Configuration;

public class AppConfiguration
{
    public string Database { get; set; } = "SqlServer";
    public JwtConfig Jwt { get; set; } = new();
    public PlaywrightConfig Playwright { get; set; } = new();
    public StorageConfig Storage { get; set; } = new();
    public NotificationsConfig Notifications { get; set; } = new();
    public SchedulingConfig Scheduling { get; set; } = new();
}

public class JwtConfig
{
    public string Secret { get; set; } = "CHANGE-THIS-TO-32-CHARS-MINIMUM-SECRET";
    public string Issuer { get; set; } = "QAStudio";
    public string Audience { get; set; } = "QAStudio";
    public int AccessTokenExpirationMinutes { get; set; } = 60;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}

public class PlaywrightConfig
{
    public string BrowsersPath { get; set; } = "./playwright-browsers";
    public int RecordTimeoutSeconds { get; set; } = 300;
    public int StepTimeoutMs { get; set; } = 30000;
    public bool VideoEnabled { get; set; } = true;
    public bool TraceEnabled { get; set; } = true;
    public bool ScreenshotOnEveryStep { get; set; } = true;
    public bool Headless { get; set; } = true;
}

public class StorageConfig
{
    public string UploadsPath { get; set; } = "./uploads";
    public string ScreenshotsPath { get; set; } = "./uploads/screenshots";
    public string VideosPath { get; set; } = "./uploads/videos";
    public string TracesPath { get; set; } = "./uploads/traces";
    public int MaxUploadSizeMb { get; set; } = 100;
}

public class NotificationsConfig
{
    public EmailConfig Email { get; set; } = new();
    public SlackConfig Slack { get; set; } = new();
}

public class EmailConfig
{
    public bool Enabled { get; set; }
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUser { get; set; } = string.Empty;
    public string SmtpPass { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
}

public class SlackConfig
{
    public bool Enabled { get; set; }
    public string WebhookUrl { get; set; } = string.Empty;
}

public class SchedulingConfig
{
    public string Provider { get; set; } = "Hangfire";
    public string CronTimezone { get; set; } = "UTC";
}
