namespace Newsletter.Data.Entities;

public class Tag : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Navigation property for the many-to-many relationship with Event
    public ICollection<Event> Events { get; set; } = new List<Event>();
} 