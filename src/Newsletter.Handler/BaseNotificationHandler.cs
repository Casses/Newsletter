using Microsoft.Extensions.Logging;
using Newsletter.Data;
using Newsletter.Data.Entities;
using Newsletter.Interface.Handler;

namespace Newsletter.Handler;

public abstract class BaseNotificationHandler : INotificationHandler
{
    protected readonly ILogger<BaseNotificationHandler> _logger;
    protected readonly NewsletterContext _context;

    protected BaseNotificationHandler(
        ILogger<BaseNotificationHandler> logger,
        NewsletterContext context)
    {
        _logger = logger;
        _context = context;
    }

    public abstract Task<bool> CanHandleAsync(NotificationType type);

    public async Task<NotificationResult> HandleAsync(NotificationRecord notification)
    {
        try
        {
            if (!await CanHandleAsync(notification.Type))
            {
                var result = new NotificationResult
                {
                    NotificationId = notification.Id,
                    Success = false,
                    ErrorMessage = $"Handler cannot process notification type: {notification.Type}",
                    DeliveryStatus = "Failed"
                };

                await _context.AddAsync(result);
                await _context.SaveChangesAsync();
                return result;
            }

            _logger.LogInformation(
                "Processing notification {NotificationId} of type {Type} for subscriber {SubscriberId}",
                notification.Id,
                notification.Type,
                notification.SubscriberId);

            var processResult = await ProcessNotificationAsync(notification);

            // Set the notification ID and save the result
            processResult.NotificationId = notification.Id;
            await _context.AddAsync(processResult);
            await _context.SaveChangesAsync();

            if (processResult.Success)
            {
                _logger.LogInformation(
                    "Successfully processed notification {NotificationId} for subscriber {SubscriberId}",
                    notification.Id,
                    notification.SubscriberId);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to process notification {NotificationId} for subscriber {SubscriberId}: {ErrorMessage}",
                    notification.Id,
                    notification.SubscriberId,
                    processResult.ErrorMessage);
            }

            return processResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing notification {NotificationId} for subscriber {SubscriberId}",
                notification.Id,
                notification.SubscriberId);

            var result = new NotificationResult
            {
                NotificationId = notification.Id,
                Success = false,
                ErrorMessage = ex.Message,
                DeliveryStatus = "Failed"
            };

            await _context.AddAsync(result);
            await _context.SaveChangesAsync();
            return result;
        }
    }

    protected abstract Task<NotificationResult> ProcessNotificationAsync(NotificationRecord notification);
} 