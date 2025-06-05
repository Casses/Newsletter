using Microsoft.EntityFrameworkCore;
using Newsletter.Data;
using Newsletter.Data.Entities;
using Newsletter.Interface.Services;

namespace Newsletter.Service;

public class TagService : ITagService
{
    private readonly NewsletterContext _context;

    public TagService(NewsletterContext context)
    {
        _context = context;
    }

    public async Task<Tag> CreateTagAsync(string name, string? description = null)
    {
        if (await TagExistsAsync(name))
            throw new InvalidOperationException($"A tag with name '{name}' already exists.");

        var tag = new Tag
        {
            Name = name.Trim(),
            Description = description?.Trim()
        };
        await _context.AddAsync(tag);
        await _context.SaveChangesAsync();
        return tag;
    }

    public async Task<Tag?> GetTagByIdAsync(Guid id)
    {
        return await _context.Set<Tag>().FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
    }

    public async Task<Tag?> GetTagByNameAsync(string name)
    {
        return await _context.Set<Tag>().FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower().Trim() && !t.IsDeleted);
    }

    public async Task<IEnumerable<Tag>> GetAllTagsAsync(bool includeInactive = false)
    {
        return await _context.Set<Tag>()
            .Where(t => !t.IsDeleted)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<Tag> UpdateTagAsync(Guid id, string name, string? description = null)
    {
        var tag = await GetTagByIdAsync(id);
        if (tag == null)
            throw new KeyNotFoundException($"Tag with ID {id} not found.");
        if (await TagExistsAsync(name) && tag.Name.ToLower() != name.ToLower().Trim())
            throw new InvalidOperationException($"A tag with name '{name}' already exists.");
        tag.Name = name.Trim();
        tag.Description = description?.Trim();
        await _context.UpdateAsync(tag);
        return tag;
    }

    public async Task DeleteTagAsync(Guid id)
    {
        await _context.DeleteAsync<Tag>(id);
    }

    public async Task<bool> TagExistsAsync(string name)
    {
        return await _context.Set<Tag>().AnyAsync(t => t.Name.ToLower() == name.ToLower().Trim() && !t.IsDeleted);
    }

    public async Task<bool> TagExistsAsync(Guid id)
    {
        return await _context.Set<Tag>().AnyAsync(t => t.Id == id && !t.IsDeleted);
    }
} 