namespace Newsletter.Data.Entities;

public class NotificationRecord : BaseEntity
{
    public Guid SubscriberId { get; set; }
    public Subscriber Subscriber { get; set; } = null!;
    
    public Guid? EventId { get; set; }
    public Event? Event { get; set; }
    
    public Guid? EventInstanceId { get; set; }
    public EventInstance? EventInstance { get; set; }
    
    public NotificationType Type { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    
    // Results of notification attempts
    public ICollection<NotificationResult> Results { get; set; } = new List<NotificationResult>();
    
    // Computed properties based on the latest result
    public bool WasSuccessful => Results.OrderByDescending(r => r.CreatedAt).FirstOrDefault()?.Success ?? false;
    public string? ErrorMessage => Results.OrderByDescending(r => r.CreatedAt).FirstOrDefault()?.ErrorMessage;
    public string? DeliveryStatus => Results.OrderByDescending(r => r.CreatedAt).FirstOrDefault()?.DeliveryStatus;
    public DateTime? DeliveredAt => Results.OrderByDescending(r => r.CreatedAt).FirstOrDefault()?.DeliveredAt;
    public DateTime? ReadAt => Results.OrderByDescending(r => r.CreatedAt).FirstOrDefault()?.ReadAt;
}

public enum NotificationType
{
    Email,
    Sms,
    Push,
    All
} 