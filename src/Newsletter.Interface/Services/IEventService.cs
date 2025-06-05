using Newsletter.Data.Entities;

namespace Newsletter.Interface.Services;

public interface IEventService
{
    // Event operations
    Task<Event> CreateEventAsync(
        string title,
        string description,
        string? externalUrl = null,
        string? imageUrl = null,
        string? organizerName = null,
        string? organizerEmail = null,
        string? organizerPhone = null,
        string? category = null,
        IEnumerable<string>? tagNames = null);

    Task<Event?> GetEventByIdAsync(Guid id);
    Task<Event?> GetEventByTitleAsync(string title);
    Task<IEnumerable<Event>> GetAllEventsAsync(bool includeInactive = false);
    Task<Event> UpdateEventAsync(
        Guid id,
        string title,
        string description,
        string? externalUrl = null,
        string? imageUrl = null,
        string? organizerName = null,
        string? organizerEmail = null,
        string? organizerPhone = null,
        string? category = null,
        IEnumerable<string>? tagNames = null);
    Task DeleteEventAsync(Guid id);
    Task PublishEventAsync(Guid id);
    Task UnpublishEventAsync(Guid id);

    // EventInstance operations
    Task<EventInstance> AddEventInstanceAsync(
        Guid eventId,
        DateTime startTime,
        DateTime endTime,
        string location,
        string? room = null,
        string? address = null,
        string? city = null,
        string? state = null,
        string? country = null,
        string? postalCode = null,
        double? latitude = null,
        double? longitude = null,
        bool isVirtual = false,
        string? virtualMeetingUrl = null,
        string? virtualMeetingPlatform = null,
        int? maxAttendees = null,
        decimal? price = null,
        string? currency = null,
        string? notes = null);

    Task<EventInstance?> GetEventInstanceByIdAsync(Guid id);
    Task<IEnumerable<EventInstance>> GetEventInstancesAsync(Guid eventId, bool includeCancelled = false);
    Task<EventInstance> UpdateEventInstanceAsync(
        Guid id,
        DateTime startTime,
        DateTime endTime,
        string location,
        string? room = null,
        string? address = null,
        string? city = null,
        string? state = null,
        string? country = null,
        string? postalCode = null,
        double? latitude = null,
        double? longitude = null,
        bool isVirtual = false,
        string? virtualMeetingUrl = null,
        string? virtualMeetingPlatform = null,
        int? maxAttendees = null,
        decimal? price = null,
        string? currency = null,
        string? notes = null);

    Task CancelEventInstanceAsync(Guid id, string reason);
    Task DeleteEventInstanceAsync(Guid id);
} 