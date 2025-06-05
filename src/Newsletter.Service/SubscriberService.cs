using Microsoft.EntityFrameworkCore;
using Newsletter.Data;
using Newsletter.Data.Entities;
using Newsletter.Interface.Services;

namespace Newsletter.Service;

public class SubscriberService : ISubscriberService
{
    private readonly NewsletterContext _context;
    private readonly ITagService _tagService;

    public SubscriberService(NewsletterContext context, ITagService tagService)
    {
        _context = context;
        _tagService = tagService;
    }

    public async Task<Subscriber> CreateSubscriberAsync(
        string email,
        string firstName,
        string lastName,
        bool isActive = true,
        string? phoneNumber = null,
        string? notes = null,
        IEnumerable<string>? preferredTagNames = null)
    {
        if (await SubscriberExistsAsync(email))
            throw new InvalidOperationException($"A subscriber with email '{email}' already exists.");

        var subscriber = new Subscriber
        {
            Email = email.Trim().ToLowerInvariant(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            IsActive = isActive,
            PhoneNumber = phoneNumber?.Trim(),
            Notes = notes?.Trim()
        };

        if (preferredTagNames != null)
        {
            foreach (var tagName in preferredTagNames)
            {
                var tag = await _tagService.GetTagByNameAsync(tagName);
                if (tag == null)
                    tag = await _tagService.CreateTagAsync(tagName);
                
                subscriber.TagPreferences.Add(new SubscriberTag
                {
                    Subscriber = subscriber,
                    Tag = tag,
                    AddedAt = DateTime.UtcNow
                });
            }
        }

        await _context.AddAsync(subscriber);
        await _context.SaveChangesAsync();
        return subscriber;
    }

    public async Task<Subscriber?> GetSubscriberByIdAsync(Guid id)
    {
        return await _context.Set<Subscriber>()
            .Include(s => s.TagPreferences)
            .ThenInclude(st => st.Tag)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
    }

    public async Task<Subscriber?> GetSubscriberByEmailAsync(string email)
    {
        return await _context.Set<Subscriber>()
            .Include(s => s.TagPreferences)
            .ThenInclude(st => st.Tag)
            .FirstOrDefaultAsync(s => s.Email.ToLower() == email.ToLower().Trim() && !s.IsDeleted);
    }

    public async Task<IEnumerable<Subscriber>> GetAllSubscribersAsync(bool includeInactive = false)
    {
        var query = _context.Set<Subscriber>()
            .Include(s => s.TagPreferences)
            .ThenInclude(st => st.Tag)
            .AsQueryable();

        if (!includeInactive)
            query = query.Where(s => s.IsActive && !s.IsDeleted);

        return await query.OrderBy(s => s.Email).ToListAsync();
    }

    public async Task<Subscriber> UpdateSubscriberAsync(
        Guid id,
        string firstName,
        string lastName,
        bool isActive,
        string? phoneNumber = null,
        string? notes = null)
    {
        var subscriber = await GetSubscriberByIdAsync(id);
        if (subscriber == null)
            throw new KeyNotFoundException($"Subscriber with ID {id} not found.");

        subscriber.FirstName = firstName.Trim();
        subscriber.LastName = lastName.Trim();
        subscriber.IsActive = isActive;
        subscriber.PhoneNumber = phoneNumber?.Trim();
        subscriber.Notes = notes?.Trim();

        await _context.UpdateAsync(subscriber);
        return subscriber;
    }

    public async Task DeleteSubscriberAsync(Guid id)
    {
        await _context.DeleteAsync<Subscriber>(id);
    }

    public async Task<bool> SubscriberExistsAsync(string email)
    {
        return await _context.Set<Subscriber>()
            .AnyAsync(s => s.Email.ToLower() == email.ToLower().Trim() && !s.IsDeleted);
    }

    public async Task<bool> SubscriberExistsAsync(Guid id)
    {
        return await _context.Set<Subscriber>()
            .AnyAsync(s => s.Id == id && !s.IsDeleted);
    }

    public async Task<IEnumerable<Tag>> GetSubscriberPreferredTagsAsync(Guid subscriberId)
    {
        var subscriber = await GetSubscriberByIdAsync(subscriberId);
        if (subscriber == null)
            throw new KeyNotFoundException($"Subscriber with ID {subscriberId} not found.");

        return subscriber.TagPreferences
            .Where(st => !st.Tag.IsDeleted)
            .Select(st => st.Tag)
            .OrderBy(t => t.Name);
    }

    public async Task AddPreferredTagAsync(Guid subscriberId, string tagName)
    {
        var subscriber = await GetSubscriberByIdAsync(subscriberId);
        if (subscriber == null)
            throw new KeyNotFoundException($"Subscriber with ID {subscriberId} not found.");

        if (await HasPreferredTagAsync(subscriberId, tagName))
            return;

        var tag = await _tagService.GetTagByNameAsync(tagName);
        if (tag == null)
            tag = await _tagService.CreateTagAsync(tagName);

        subscriber.TagPreferences.Add(new SubscriberTag
        {
            Subscriber = subscriber,
            Tag = tag,
            AddedAt = DateTime.UtcNow
        });

        await _context.UpdateAsync(subscriber);
    }

    public async Task RemovePreferredTagAsync(Guid subscriberId, string tagName)
    {
        var subscriber = await GetSubscriberByIdAsync(subscriberId);
        if (subscriber == null)
            throw new KeyNotFoundException($"Subscriber with ID {subscriberId} not found.");

        var tag = await _tagService.GetTagByNameAsync(tagName);
        if (tag == null)
            return;

        var preference = subscriber.TagPreferences.FirstOrDefault(st => st.TagId == tag.Id);
        if (preference != null)
        {
            subscriber.TagPreferences.Remove(preference);
            await _context.UpdateAsync(subscriber);
        }
    }

    public async Task UpdatePreferredTagsAsync(Guid subscriberId, IEnumerable<string> tagNames)
    {
        var subscriber = await GetSubscriberByIdAsync(subscriberId);
        if (subscriber == null)
            throw new KeyNotFoundException($"Subscriber with ID {subscriberId} not found.");

        // Clear existing preferences
        subscriber.TagPreferences.Clear();

        // Add new preferences
        foreach (var tagName in tagNames)
        {
            var tag = await _tagService.GetTagByNameAsync(tagName);
            if (tag == null)
                tag = await _tagService.CreateTagAsync(tagName);

            subscriber.TagPreferences.Add(new SubscriberTag
            {
                Subscriber = subscriber,
                Tag = tag,
                AddedAt = DateTime.UtcNow
            });
        }

        await _context.UpdateAsync(subscriber);
    }

    public async Task<bool> HasPreferredTagAsync(Guid subscriberId, string tagName)
    {
        var subscriber = await GetSubscriberByIdAsync(subscriberId);
        if (subscriber == null)
            throw new KeyNotFoundException($"Subscriber with ID {subscriberId} not found.");

        var tag = await _tagService.GetTagByNameAsync(tagName);
        if (tag == null)
            return false;

        return subscriber.TagPreferences.Any(st => st.TagId == tag.Id);
    }
} 