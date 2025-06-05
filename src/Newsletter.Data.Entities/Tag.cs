namespace Newsletter.Data.Entities;

public class Tag : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Navigation properties
    public ICollection<Event> Events { get; set; } = new List<Event>();
    public ICollection<SubscriberTag> SubscriberPreferences { get; set; } = new List<SubscriberTag>();
} 