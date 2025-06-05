namespace Newsletter.CompositionRoot;

public class NewsletterOptions
{
    public const string SectionName = "Newsletter";

    public DatabaseOptions Database { get; set; } = new();
    public NotificationOptions Notifications { get; set; } = new();
}

public class DatabaseOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public bool EnableRetryOnFailure { get; set; } = true;
    public int MaxRetryCount { get; set; } = 3;
    public int CommandTimeout { get; set; } = 30;
}

public class NotificationOptions
{
    public EmailOptions Email { get; set; } = new();
    public SmsOptions Sms { get; set; } = new();
    public PushOptions Push { get; set; } = new();
}

public class EmailOptions
{
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string? Username { get; set; }
    public string? Password { get; set; }
}

public class SmsOptions
{
    public string Provider { get; set; } = string.Empty;
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string FromNumber { get; set; } = string.Empty;
}

public class PushOptions
{
    public string Provider { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public string? PrivateKey { get; set; }
    public string? ClientEmail { get; set; }
} 