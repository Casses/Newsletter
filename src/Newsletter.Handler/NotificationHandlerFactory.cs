using Microsoft.Extensions.Logging;
using Newsletter.Data.Entities;
using Newsletter.Interface.Handler;

namespace Newsletter.Handler;

public interface INotificationHandlerFactory
{
    Task<IEnumerable<INotificationHandler>> GetHandlersAsync(NotificationType type);
}

public class NotificationHandlerFactory : INotificationHandlerFactory
{
    private readonly IEnumerable<INotificationHandler> _handlers;
    private readonly ILogger<NotificationHandlerFactory> _logger;

    public NotificationHandlerFactory(
        IEnumerable<INotificationHandler> handlers,
        ILogger<NotificationHandlerFactory> logger)
    {
        _handlers = handlers;
        _logger = logger;
    }

    public async Task<IEnumerable<INotificationHandler>> GetHandlersAsync(NotificationType type)
    {
        var handlers = new List<INotificationHandler>();

        foreach (var handler in _handlers)
        {
            if (await handler.CanHandleAsync(type))
            {
                handlers.Add(handler);
            }
        }

        if (!handlers.Any())
        {
            _logger.LogWarning("No handlers found for notification type: {Type}", type);
        }

        return handlers;
    }
} 