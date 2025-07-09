using Newsletter.Data.Entities;

namespace Newsletter.Interface.Handler;
 
public interface INotificationHandler
{
    Task<bool> CanHandleAsync(NotificationType type);
    Task<NotificationResult> HandleAsync(NotificationRecord notification);
} 