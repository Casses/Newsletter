using Newsletter.Data.Entities;

namespace Newsletter.Interface.Handler;
 
public interface INotificationHandlerFactory
{
    Task<IEnumerable<INotificationHandler>> GetHandlersAsync(NotificationType type);
} 