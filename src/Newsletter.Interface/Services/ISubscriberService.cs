using Newsletter.Data.Entities;

namespace Newsletter.Interface.Services;

public interface ISubscriberService
{
    Task<Subscriber> CreateSubscriberAsync(
        string email,
        string firstName,
        string lastName,
        bool isActive = true,
        string? phoneNumber = null,
        string? notes = null,
        IEnumerable<string>? preferredTagNames = null);

    Task<Subscriber?> GetSubscriberByIdAsync(Guid id);
    Task<Subscriber?> GetSubscriberByEmailAsync(string email);
    Task<IEnumerable<Subscriber>> GetAllSubscribersAsync(bool includeInactive = false);
    
    Task<Subscriber> UpdateSubscriberAsync(
        Guid id,
        string firstName,
        string lastName,
        bool isActive,
        string? phoneNumber = null,
        string? notes = null);

    Task DeleteSubscriberAsync(Guid id);
    Task<bool> SubscriberExistsAsync(string email);
    Task<bool> SubscriberExistsAsync(Guid id);

    // Tag preference management
    Task<IEnumerable<Tag>> GetSubscriberPreferredTagsAsync(Guid subscriberId);
    Task AddPreferredTagAsync(Guid subscriberId, string tagName);
    Task RemovePreferredTagAsync(Guid subscriberId, string tagName);
    Task UpdatePreferredTagsAsync(Guid subscriberId, IEnumerable<string> tagNames);
    Task<bool> HasPreferredTagAsync(Guid subscriberId, string tagName);
} 