using Microsoft.EntityFrameworkCore;
using Newsletter.Data;
using Newsletter.Data.Entities;
using Newsletter.Interface.Services;

namespace Newsletter.Service;

public class EventService : IEventService
{
    private readonly NewsletterContext _context;
    private readonly ITagService _tagService;

    public EventService(NewsletterContext context, ITagService tagService)
    {
        _context = context;
        _tagService = tagService;
    }

    public async Task<Event> CreateEventAsync(
        string title,
        string description,
        string? externalUrl = null,
        string? imageUrl = null,
        string? organizerName = null,
        string? organizerEmail = null,
        string? organizerPhone = null,
        string? category = null,
        IEnumerable<string>? tagNames = null)
    {
        if (await GetEventByTitleAsync(title) != null)
            throw new InvalidOperationException($"An event with title '{title}' already exists.");

        var @event = new Event
        {
            Title = title.Trim(),
            Description = description.Trim(),
            ExternalUrl = externalUrl?.Trim(),
            ImageUrl = imageUrl?.Trim(),
            OrganizerName = organizerName?.Trim(),
            OrganizerEmail = organizerEmail?.Trim(),
            OrganizerPhone = organizerPhone?.Trim(),
            Category = category?.Trim()
        };

        if (tagNames != null)
        {
            foreach (var tagName in tagNames)
            {
                var tag = await _tagService.GetTagByNameAsync(tagName);
                if (tag == null)
                    tag = await _tagService.CreateTagAsync(tagName);
                @event.Tags.Add(tag);
            }
        }

        await _context.AddAsync(@event);
        await _context.SaveChangesAsync();
        return @event;
    }

    public async Task<Event?> GetEventByIdAsync(Guid id)
    {
        return await _context.Set<Event>()
            .Include(e => e.Tags)
            .Include(e => e.Occurrences)
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
    }

    public async Task<Event?> GetEventByTitleAsync(string title)
    {
        return await _context.Set<Event>()
            .Include(e => e.Tags)
            .Include(e => e.Occurrences)
            .FirstOrDefaultAsync(e => e.Title.ToLower() == title.ToLower().Trim() && !e.IsDeleted);
    }

    public async Task<IEnumerable<Event>> GetAllEventsAsync(bool includeInactive = false)
    {
        var query = _context.Set<Event>()
            .Include(e => e.Tags)
            .Include(e => e.Occurrences)
            .AsQueryable();

        if (!includeInactive)
            query = query.Where(e => !e.IsDeleted);

        return await query.OrderBy(e => e.Title).ToListAsync();
    }

    public async Task<Event> UpdateEventAsync(
        Guid id,
        string title,
        string description,
        string? externalUrl = null,
        string? imageUrl = null,
        string? organizerName = null,
        string? organizerEmail = null,
        string? organizerPhone = null,
        string? category = null,
        IEnumerable<string>? tagNames = null)
    {
        var @event = await GetEventByIdAsync(id);
        if (@event == null)
            throw new KeyNotFoundException($"Event with ID {id} not found.");

        var existingEvent = await GetEventByTitleAsync(title);
        if (existingEvent != null && existingEvent.Id != id)
            throw new InvalidOperationException($"An event with title '{title}' already exists.");

        @event.Title = title.Trim();
        @event.Description = description.Trim();
        @event.ExternalUrl = externalUrl?.Trim();
        @event.ImageUrl = imageUrl?.Trim();
        @event.OrganizerName = organizerName?.Trim();
        @event.OrganizerEmail = organizerEmail?.Trim();
        @event.OrganizerPhone = organizerPhone?.Trim();
        @event.Category = category?.Trim();

        if (tagNames != null)
        {
            @event.Tags.Clear();
            foreach (var tagName in tagNames)
            {
                var tag = await _tagService.GetTagByNameAsync(tagName);
                if (tag == null)
                    tag = await _tagService.CreateTagAsync(tagName);
                @event.Tags.Add(tag);
            }
        }

        await _context.UpdateAsync(@event);
        return @event;
    }

    public async Task DeleteEventAsync(Guid id)
    {
        await _context.DeleteAsync<Event>(id);
    }

    public async Task PublishEventAsync(Guid id)
    {
        var @event = await GetEventByIdAsync(id);
        if (@event == null)
            throw new KeyNotFoundException($"Event with ID {id} not found.");

        @event.IsPublished = true;
        @event.PublishedAt = DateTime.UtcNow;
        await _context.UpdateAsync(@event);
    }

    public async Task UnpublishEventAsync(Guid id)
    {
        var @event = await GetEventByIdAsync(id);
        if (@event == null)
            throw new KeyNotFoundException($"Event with ID {id} not found.");

        @event.IsPublished = false;
        @event.PublishedAt = null;
        await _context.UpdateAsync(@event);
    }

    public async Task<EventInstance> AddEventInstanceAsync(
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
        string? notes = null)
    {
        var @event = await GetEventByIdAsync(eventId);
        if (@event == null)
            throw new KeyNotFoundException($"Event with ID {eventId} not found.");

        if (startTime >= endTime)
            throw new ArgumentException("Start time must be before end time.");

        var instance = new EventInstance
        {
            EventId = eventId,
            StartTime = startTime,
            EndTime = endTime,
            Location = location.Trim(),
            Room = room?.Trim(),
            Address = address?.Trim(),
            City = city?.Trim(),
            State = state?.Trim(),
            Country = country?.Trim(),
            PostalCode = postalCode?.Trim(),
            Latitude = latitude,
            Longitude = longitude,
            IsVirtual = isVirtual,
            VirtualMeetingUrl = virtualMeetingUrl?.Trim(),
            VirtualMeetingPlatform = virtualMeetingPlatform?.Trim(),
            MaxAttendees = maxAttendees,
            Price = price,
            Currency = currency?.Trim(),
            Notes = notes?.Trim()
        };

        await _context.AddAsync(instance);
        await _context.SaveChangesAsync();
        return instance;
    }

    public async Task<EventInstance?> GetEventInstanceByIdAsync(Guid id)
    {
        return await _context.Set<EventInstance>()
            .Include(i => i.Event)
            .ThenInclude(e => e!.Tags)
            .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
    }

    public async Task<IEnumerable<EventInstance>> GetEventInstancesAsync(Guid eventId, bool includeCancelled = false)
    {
        var query = _context.Set<EventInstance>()
            .Include(i => i.Event)
            .ThenInclude(e => e!.Tags)
            .Where(i => i.EventId == eventId && !i.IsDeleted);

        if (!includeCancelled)
            query = query.Where(i => !i.IsCancelled);

        return await query.OrderBy(i => i.StartTime).ToListAsync();
    }

    public async Task<EventInstance> UpdateEventInstanceAsync(
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
        string? notes = null)
    {
        var instance = await GetEventInstanceByIdAsync(id);
        if (instance == null)
            throw new KeyNotFoundException($"Event instance with ID {id} not found.");

        if (startTime >= endTime)
            throw new ArgumentException("Start time must be before end time.");

        instance.StartTime = startTime;
        instance.EndTime = endTime;
        instance.Location = location.Trim();
        instance.Room = room?.Trim();
        instance.Address = address?.Trim();
        instance.City = city?.Trim();
        instance.State = state?.Trim();
        instance.Country = country?.Trim();
        instance.PostalCode = postalCode?.Trim();
        instance.Latitude = latitude;
        instance.Longitude = longitude;
        instance.IsVirtual = isVirtual;
        instance.VirtualMeetingUrl = virtualMeetingUrl?.Trim();
        instance.VirtualMeetingPlatform = virtualMeetingPlatform?.Trim();
        instance.MaxAttendees = maxAttendees;
        instance.Price = price;
        instance.Currency = currency?.Trim();
        instance.Notes = notes?.Trim();

        await _context.UpdateAsync(instance);
        return instance;
    }

    public async Task CancelEventInstanceAsync(Guid id, string reason)
    {
        var instance = await GetEventInstanceByIdAsync(id);
        if (instance == null)
            throw new KeyNotFoundException($"Event instance with ID {id} not found.");

        instance.IsCancelled = true;
        instance.CancellationReason = reason.Trim();
        instance.CancelledAt = DateTime.UtcNow;

        await _context.UpdateAsync(instance);
    }

    public async Task DeleteEventInstanceAsync(Guid id)
    {
        await _context.DeleteAsync<EventInstance>(id);
    }
} 