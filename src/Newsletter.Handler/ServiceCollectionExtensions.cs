using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newsletter.Interface.Handler;

namespace Newsletter.Handler;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNotificationHandlers(this IServiceCollection services)
    {
        // Register handlers
        services.TryAddScoped<EmailNotificationHandler>();
        services.TryAddScoped<SmsNotificationHandler>();
        services.TryAddScoped<PushNotificationHandler>();

        // Register handler factory
        services.TryAddScoped<INotificationHandlerFactory, NotificationHandlerFactory>();

        // Register handlers as INotificationHandler
        services.TryAddScoped<INotificationHandler>(sp => sp.GetRequiredService<EmailNotificationHandler>());
        services.TryAddScoped<INotificationHandler>(sp => sp.GetRequiredService<SmsNotificationHandler>());
        services.TryAddScoped<INotificationHandler>(sp => sp.GetRequiredService<PushNotificationHandler>());

        return services;
    }
} 