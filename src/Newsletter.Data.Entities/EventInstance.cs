namespace Newsletter.Data.Entities;

public class EventInstance : BaseEntity
{
    public Guid EventId { get; set; }
    public Event Event { get; set; } = null!;
    
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? Room { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    
    public bool IsVirtual { get; set; }
    public string? VirtualMeetingUrl { get; set; }
    public string? VirtualMeetingPlatform { get; set; }
    
    public int? MaxAttendees { get; set; }
    public int CurrentAttendeeCount { get; set; }
    
    public decimal? Price { get; set; }
    public string? Currency { get; set; }
    
    public bool IsCancelled { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? CancelledAt { get; set; }
    
    public string? Notes { get; set; }
} 