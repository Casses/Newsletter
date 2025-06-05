using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newsletter.Data;
using Newsletter.Handler;
using Newsletter.Interface.Handler;
using Newsletter.Interface.Services;
using Newsletter.Service;

namespace Newsletter.CompositionRoot;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNewsletterServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure options
        services.Configure<NewsletterOptions>(options =>
        {
            configuration.GetSection(NewsletterOptions.SectionName).Bind(options);
        });

        var options = new NewsletterOptions();
        configuration.GetSection(NewsletterOptions.SectionName).Bind(options);

        if (string.IsNullOrEmpty(options.Database.ConnectionString))
            throw new InvalidOperationException(
                $"Configuration section '{NewsletterOptions.SectionName}:Database:ConnectionString' is missing or invalid.");

        // Register DbContext
        services.AddDbContext<NewsletterContext>(dbOptions =>
        {
            dbOptions.UseSqlServer(
                options.Database.ConnectionString,
                sqlOptions =>
                {
                    if (options.Database.EnableRetryOnFailure)
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: options.Database.MaxRetryCount);
                    }
                    sqlOptions.CommandTimeout(options.Database.CommandTimeout);
                });
        });

        // Register Services
        services.AddScoped<ISubscriberService, SubscriberService>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<INotificationService, NotificationService>();

        // Register Handlers
        services.AddNotificationHandlers();

        // Register Options
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<NewsletterOptions>>().Value);

        return services;
    }

    public static IServiceCollection AddNewsletterServicesForTesting(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> dbContextOptions)
    {
        // Register DbContext with test configuration
        services.AddDbContext<NewsletterContext>(dbContextOptions);

        // Register Services
        services.AddScoped<ISubscriberService, SubscriberService>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<INotificationService, NotificationService>();

        // Register Handlers
        services.AddNotificationHandlers();

        // Register default options for testing
        services.Configure<NewsletterOptions>(options =>
        {
            options.Database.EnableRetryOnFailure = false;
            options.Database.CommandTimeout = 5;
        });

        return services;
    }
} 