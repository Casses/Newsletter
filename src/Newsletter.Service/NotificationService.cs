using Microsoft.EntityFrameworkCore;
using Newsletter.Data;
using Newsletter.Data.Entities;
using Newsletter.Interface.Handler;
using Newsletter.Interface.Services;

namespace Newsletter.Service;

public class NotificationService : INotificationService
{
    private readonly NewsletterContext _context;
    private readonly ISubscriberService _subscriberService;
    private readonly IEventService _eventService;
    private readonly ITagService _tagService;
    private readonly INotificationHandlerFactory _handlerFactory;

    public NotificationService(
        NewsletterContext context,
        ISubscriberService subscriberService,
        IEventService eventService,
        ITagService tagService,
        INotificationHandlerFactory handlerFactory)
    {
        _context = context;
        _subscriberService = subscriberService;
        _eventService = eventService;
        _tagService = tagService;
        _handlerFactory = handlerFactory;
    }

    public async Task UpdateNotificationPreferencesAsync(
        Guid subscriberId,
        bool prefersEmail,
        bool prefersSms,
        bool prefersPush)
    {
        var subscriber = await _subscriberService.GetSubscriberByIdAsync(subscriberId);
        if (subscriber == null)
            throw new KeyNotFoundException($"Subscriber with ID {subscriberId} not found.");

        subscriber.PrefersEmail = prefersEmail;
        subscriber.PrefersSms = prefersSms;
        subscriber.PrefersPush = prefersPush;

        await _context.UpdateAsync(subscriber);
    }

    public async Task NotifySubscribersAboutEventAsync(
        Guid eventId,
        NotificationType type,
        string? customMessage = null,
        IEnumerable<string>? requiredTagNames = null,
        bool notifyOnlyActiveSubscribers = true)
    {
        var @event = await _eventService.GetEventByIdAsync(eventId);
        if (@event == null)
            throw new KeyNotFoundException($"Event with ID {eventId} not found.");

        var subscribers = await GetEligibleSubscribersAsync(
            requiredTagNames,
            notifyOnlyActiveSubscribers);

        var subject = $"New Event: {@event.Title}";
        var message = customMessage ?? GenerateEventNotificationMessage(@event);

        foreach (var subscriber in subscribers)
        {
            if (await ShouldNotifySubscriberAsync(subscriber, @event.Id, type))
            {
                await SendNotificationAsync(
                    subscriber,
                    subject,
                    message,
                    type,
                    eventId: @event.Id);
            }
        }
    }

    public async Task NotifySubscribersAboutEventInstanceAsync(
        Guid eventInstanceId,
        NotificationType type,
        string? customMessage = null,
        IEnumerable<string>? requiredTagNames = null,
        bool notifyOnlyActiveSubscribers = true)
    {
        var instance = await _eventService.GetEventInstanceByIdAsync(eventInstanceId);
        if (instance == null)
            throw new KeyNotFoundException($"Event instance with ID {eventInstanceId} not found.");

        var subscribers = await GetEligibleSubscribersAsync(
            requiredTagNames,
            notifyOnlyActiveSubscribers);

        var subject = $"Event Reminder: {instance.Event?.Title}";
        var message = customMessage ?? GenerateEventInstanceNotificationMessage(instance);

        foreach (var subscriber in subscribers)
        {
            if (await ShouldNotifySubscriberAboutInstanceAsync(subscriber, instance.Id, type))
            {
                await SendNotificationAsync(
                    subscriber,
                    subject,
                    message,
                    type,
                    eventInstanceId: instance.Id);
            }
        }
    }

    public async Task NotifySubscribersByTagsAsync(
        IEnumerable<string> tagNames,
        string subject,
        string message,
        NotificationType type,
        bool notifyOnlyActiveSubscribers = true)
    {
        var subscribers = await GetEligibleSubscribersAsync(
            tagNames,
            notifyOnlyActiveSubscribers);

        foreach (var subscriber in subscribers)
        {
            await SendNotificationAsync(
                subscriber,
                subject,
                message,
                type);
        }
    }

    public async Task NotifySubscribersByLocationAsync(
        string subject,
        string message,
        NotificationType type,
        string city,
        string? state = null,
        string? country = null,
        double? radiusMiles = null,
        bool notifyOnlyActiveSubscribers = true)
    {
        var query = _context.Set<Subscriber>()
            .Include(s => s.TagPreferences)
            .ThenInclude(st => st.Tag)
            .AsQueryable();

        if (!notifyOnlyActiveSubscribers)
            query = query.Where(s => s.IsActive && !s.IsDeleted);

        // Filter by location
        query = query.Where(s => s.PreferredCity == city);
        if (state != null)
            query = query.Where(s => s.PreferredState == state);
        if (country != null)
            query = query.Where(s => s.PreferredCountry == country);

        // TODO: Implement radius-based filtering when coordinates are available
        if (radiusMiles.HasValue && radiusMiles.Value > 0)
        {
            // This would require geospatial queries
            // For now, we'll just log that this feature is not implemented
            Console.WriteLine("Radius-based filtering is not yet implemented");
        }

        var subscribers = await query.ToListAsync();

        foreach (var subscriber in subscribers)
        {
            await SendNotificationAsync(
                subscriber,
                subject,
                message,
                type);
        }
    }

    public async Task<IEnumerable<NotificationRecord>> GetSubscriberNotificationHistoryAsync(
        Guid subscriberId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        NotificationType? type = null)
    {
        var query = _context.Set<NotificationRecord>()
            .Where(n => n.SubscriberId == subscriberId);

        if (startDate.HasValue)
            query = query.Where(n => n.SentAt >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(n => n.SentAt <= endDate.Value);
        if (type.HasValue)
            query = query.Where(n => n.Type == type.Value);

        return await query.OrderByDescending(n => n.SentAt).ToListAsync();
    }

    public async Task<IEnumerable<NotificationRecord>> GetEventNotificationHistoryAsync(
        Guid eventId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        NotificationType? type = null)
    {
        var query = _context.Set<NotificationRecord>()
            .Where(n => n.EventId == eventId);

        if (startDate.HasValue)
            query = query.Where(n => n.SentAt >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(n => n.SentAt <= endDate.Value);
        if (type.HasValue)
            query = query.Where(n => n.Type == type.Value);

        return await query.OrderByDescending(n => n.SentAt).ToListAsync();
    }

    public async Task<bool> HasSubscriberBeenNotifiedAsync(
        Guid subscriberId,
        Guid eventId,
        NotificationType type)
    {
        return await _context.Set<NotificationRecord>()
            .AnyAsync(n => n.SubscriberId == subscriberId &&
                          n.EventId == eventId &&
                          n.Type == type &&
                          n.WasSuccessful);
    }

    public async Task<bool> HasSubscriberBeenNotifiedAboutInstanceAsync(
        Guid subscriberId,
        Guid eventInstanceId,
        NotificationType type)
    {
        return await _context.Set<NotificationRecord>()
            .AnyAsync(n => n.SubscriberId == subscriberId &&
                          n.EventInstanceId == eventInstanceId &&
                          n.Type == type &&
                          n.WasSuccessful);
    }

    #region Private Helper Methods

    private async Task<IEnumerable<Subscriber>> GetEligibleSubscribersAsync(
        IEnumerable<string>? requiredTagNames,
        bool notifyOnlyActiveSubscribers)
    {
        var query = _context.Set<Subscriber>()
            .Include(s => s.TagPreferences)
            .ThenInclude(st => st.Tag)
            .AsQueryable();

        if (notifyOnlyActiveSubscribers)
            query = query.Where(s => s.IsActive && !s.IsDeleted);

        if (requiredTagNames != null && requiredTagNames.Any())
        {
            var tagIds = new List<Guid>();
            foreach (var tagName in requiredTagNames)
            {
                var tag = await _tagService.GetTagByNameAsync(tagName);
                if (tag != null)
                    tagIds.Add(tag.Id);
            }

            if (tagIds.Any())
            {
                query = query.Where(s => s.TagPreferences
                    .Any(st => tagIds.Contains(st.TagId) && st.IsActive));
            }
        }

        return await query.ToListAsync();
    }

    private async Task<bool> ShouldNotifySubscriberAsync(
        Subscriber subscriber,
        Guid eventId,
        NotificationType type)
    {
        if (!subscriber.IsActive || subscriber.IsDeleted)
            return false;

        if (await HasSubscriberBeenNotifiedAsync(subscriber.Id, eventId, type))
            return false;

        return type switch
        {
            NotificationType.Email => subscriber.PrefersEmail,
            NotificationType.Sms => subscriber.PrefersSms,
            NotificationType.Push => subscriber.PrefersPush,
            NotificationType.All => subscriber.PrefersEmail || subscriber.PrefersSms || subscriber.PrefersPush,
            _ => false
        };
    }

    private async Task<bool> ShouldNotifySubscriberAboutInstanceAsync(
        Subscriber subscriber,
        Guid eventInstanceId,
        NotificationType type)
    {
        if (!subscriber.IsActive || subscriber.IsDeleted)
            return false;

        if (await HasSubscriberBeenNotifiedAboutInstanceAsync(subscriber.Id, eventInstanceId, type))
            return false;

        return type switch
        {
            NotificationType.Email => subscriber.PrefersEmail,
            NotificationType.Sms => subscriber.PrefersSms,
            NotificationType.Push => subscriber.PrefersPush,
            NotificationType.All => subscriber.PrefersEmail || subscriber.PrefersSms || subscriber.PrefersPush,
            _ => false
        };
    }

    private async Task SendNotificationAsync(
        Subscriber subscriber,
        string subject,
        string message,
        NotificationType type,
        Guid? eventId = null,
        Guid? eventInstanceId = null)
    {
        var notification = new NotificationRecord
        {
            SubscriberId = subscriber.Id,
            EventId = eventId,
            EventInstanceId = eventInstanceId,
            Type = type,
            Subject = subject,
            Message = message,
            SentAt = DateTime.UtcNow
        };

        await _context.AddAsync(notification);
        await _context.SaveChangesAsync();

        try
        {
            var handlers = await _handlerFactory.GetHandlersAsync(type);
            if (!handlers.Any())
            {
                var result = new NotificationResult
                {
                    NotificationId = notification.Id,
                    Success = false,
                    ErrorMessage = $"No handlers found for notification type: {type}",
                    DeliveryStatus = "Failed"
                };
                await _context.AddAsync(result);
                await _context.SaveChangesAsync();
                return;
            }

            foreach (var handler in handlers)
            {
                var result = await handler.HandleAsync(notification);
                if (result.Success)
                {
                    // Update subscriber's last notification timestamp
                    subscriber.LastNotifiedAt = DateTime.UtcNow;
                    switch (type)
                    {
                        case NotificationType.Email:
                            subscriber.LastEmailSentAt = DateTime.UtcNow;
                            break;
                        case NotificationType.Sms:
                            subscriber.LastSmsSentAt = DateTime.UtcNow;
                            break;
                    }
                    await _context.UpdateAsync(subscriber);
                }
            }
        }
        catch (Exception ex)
        {
            var result = new NotificationResult
            {
                NotificationId = notification.Id,
                Success = false,
                ErrorMessage = ex.Message,
                DeliveryStatus = "Failed"
            };
            await _context.AddAsync(result);
            await _context.SaveChangesAsync();
            throw;
        }
    }

    private string GenerateEventNotificationMessage(Event @event)
    {
        return $"""
            New Event: {@event.Title}
            
            Description: {@event.Description}
            
            {(@event.ExternalUrl != null ? $"More information: {@event.ExternalUrl}" : "")}
            
            {(@event.OrganizerName != null ? $"Organizer: {@event.OrganizerName}" : "")}
            {(@event.OrganizerEmail != null ? $"Contact: {@event.OrganizerEmail}" : "")}
            {(@event.OrganizerPhone != null ? $"Phone: {@event.OrganizerPhone}" : "")}
            
            {(@event.Tags.Any() ? $"Tags: {string.Join(", ", @event.Tags.Select(t => t.Name))}" : "")}
            """;
    }

    private string GenerateEventInstanceNotificationMessage(EventInstance instance)
    {
        var @event = instance.Event;
        if (@event == null)
            throw new InvalidOperationException("Event instance has no associated event.");

        return $"""
            Event Reminder: {@event.Title}
            
            When: {instance.StartTime:g} - {instance.EndTime:g}
            Where: {instance.Location}
            {(!string.IsNullOrEmpty(instance.Room) ? $"Room: {instance.Room}" : "")}
            
            {(@event.ExternalUrl != null ? $"More information: {@event.ExternalUrl}" : "")}
            
            {(@event.OrganizerName != null ? $"Organizer: {@event.OrganizerName}" : "")}
            {(@event.OrganizerEmail != null ? $"Contact: {@event.OrganizerEmail}" : "")}
            {(@event.OrganizerPhone != null ? $"Phone: {@event.OrganizerPhone}" : "")}
            
            {(@event.Tags.Any() ? $"Tags: {string.Join(", ", @event.Tags.Select(t => t.Name))}" : "")}
            """;
    }

    #endregion
} 