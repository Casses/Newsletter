using Newsletter.Data.Entities;

namespace Newsletter.Interface.Services;

public interface INotificationService
{
    // Notification preferences
    Task UpdateNotificationPreferencesAsync(
        Guid subscriberId,
        bool prefersEmail,
        bool prefersSms,
        bool prefersPush);

    // Event notifications
    Task NotifySubscribersAboutEventAsync(
        Guid eventId,
        NotificationType type,
        string? customMessage = null,
        IEnumerable<string>? requiredTagNames = null,
        bool notifyOnlyActiveSubscribers = true);

    Task NotifySubscribersAboutEventInstanceAsync(
        Guid eventInstanceId,
        NotificationType type,
        string? customMessage = null,
        IEnumerable<string>? requiredTagNames = null,
        bool notifyOnlyActiveSubscribers = true);

    // Bulk notifications
    Task NotifySubscribersByTagsAsync(
        IEnumerable<string> tagNames,
        string subject,
        string message,
        NotificationType type,
        bool notifyOnlyActiveSubscribers = true);

    Task NotifySubscribersByLocationAsync(
        string subject,
        string message,
        NotificationType type,
        string city,
        string? state = null,
        string? country = null,
        double? radiusMiles = null,
        bool notifyOnlyActiveSubscribers = true);

    // Notification history
    Task<IEnumerable<NotificationRecord>> GetSubscriberNotificationHistoryAsync(
        Guid subscriberId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        NotificationType? type = null);

    Task<IEnumerable<NotificationRecord>> GetEventNotificationHistoryAsync(
        Guid eventId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        NotificationType? type = null);

    // Notification status
    Task<bool> HasSubscriberBeenNotifiedAsync(
        Guid subscriberId,
        Guid eventId,
        NotificationType type);

    Task<bool> HasSubscriberBeenNotifiedAboutInstanceAsync(
        Guid subscriberId,
        Guid eventInstanceId,
        NotificationType type);
} 