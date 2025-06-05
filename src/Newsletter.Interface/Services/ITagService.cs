using Newsletter.Data.Entities;

namespace Newsletter.Interface.Services;

public interface ITagService
{
    Task<Tag> CreateTagAsync(string name, string? description = null);
    Task<Tag?> GetTagByIdAsync(Guid id);
    Task<Tag?> GetTagByNameAsync(string name);
    Task<IEnumerable<Tag>> GetAllTagsAsync(bool includeInactive = false);
    Task<Tag> UpdateTagAsync(Guid id, string name, string? description = null);
    Task DeleteTagAsync(Guid id);
    Task<bool> TagExistsAsync(string name);
    Task<bool> TagExistsAsync(Guid id);
} 