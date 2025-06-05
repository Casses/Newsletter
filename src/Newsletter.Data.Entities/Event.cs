namespace Newsletter.Data.Entities;

public class Event : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? ExternalUrl { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? ImageUrl { get; set; }
    public int? MaxAttendees { get; set; }
    public decimal? Price { get; set; }
    public string? Currency { get; set; }
    public string? OrganizerName { get; set; }
    public string? OrganizerEmail { get; set; }
    public string? OrganizerPhone { get; set; }
    public string? Category { get; set; }
    
    // Navigation properties
    public ICollection<EventInstance> Occurrences { get; set; } = new List<EventInstance>();
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
} 