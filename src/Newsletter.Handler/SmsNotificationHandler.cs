using Microsoft.Extensions.Logging;
using Newsletter.Data;
using Newsletter.Data.Entities;
using Newsletter.Interface.Handler;

namespace Newsletter.Handler;

public class SmsNotificationHandler : BaseNotificationHandler
{
    public SmsNotificationHandler(
        ILogger<SmsNotificationHandler> logger,
        NewsletterContext context)
        : base(logger, context)
    {
    }

    public override Task<bool> CanHandleAsync(NotificationType type)
    {
        return Task.FromResult(type == NotificationType.Sms || type == NotificationType.All);
    }

    protected override async Task<NotificationResult> ProcessNotificationAsync(NotificationRecord notification)
    {
        if (string.IsNullOrEmpty(notification.Subscriber?.PhoneNumber))
        {
            return new NotificationResult
            {
                Success = false,
                ErrorMessage = "Subscriber has no phone number",
                DeliveryStatus = "Failed"
            };
        }

        try
        {
            // TODO: Implement actual SMS sending logic using a service like Twilio, MessageBird, etc.
            // For now, we'll simulate sending an SMS
            await Task.Delay(100); // Simulate network delay

            _logger.LogInformation(
                "Sending SMS to {PhoneNumber}",
                notification.Subscriber.PhoneNumber);

            // Simulate successful delivery
            return new NotificationResult
            {
                Success = true,
                DeliveryStatus = "Delivered",
                DeliveredAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send SMS to {PhoneNumber}",
                notification.Subscriber.PhoneNumber);

            return new NotificationResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                DeliveryStatus = "Failed"
            };
        }
    }
} 