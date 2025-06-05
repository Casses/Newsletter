namespace Newsletter.Data.Entities;

public class SubscriberTag : BaseEntity
{
    public Guid SubscriberId { get; set; }
    public Subscriber Subscriber { get; set; } = null!;
    
    public Guid TagId { get; set; }
    public Tag Tag { get; set; } = null!;
    
    // Additional metadata about the relationship
    public bool IsActive { get; set; } = true;
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastNotifiedAt { get; set; }
    public int PreferenceLevel { get; set; } = 1; // 1-5 scale, 5 being highest interest
    public string? Notes { get; set; }
} 