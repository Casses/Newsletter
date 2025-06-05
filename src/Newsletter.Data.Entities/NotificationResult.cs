namespace Newsletter.Data.Entities;

public class NotificationResult : BaseEntity
{
    public Guid NotificationId { get; set; }
    public NotificationRecord Notification { get; set; } = null!;
    
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? DeliveryStatus { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ReadAt { get; set; }
    
    // Additional metadata
    public string? ProviderResponse { get; set; }  // Raw response from the notification provider
    public string? ProviderMessageId { get; set; } // External ID from the provider (e.g., email message ID)
    public int? RetryCount { get; set; }
    public DateTime? LastRetryAt { get; set; }
    public string? RetryReason { get; set; }
} 