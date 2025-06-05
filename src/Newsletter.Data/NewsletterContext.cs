using Microsoft.EntityFrameworkCore;
using Newsletter.Data.Entities;
using System.Linq.Expressions;

namespace Newsletter.Data;

public class NewsletterContext : DbContext
{
    public NewsletterContext(DbContextOptions<NewsletterContext> options)
        : base(options)
    {
    }

    private DbSet<T> GetDbSet<T>() where T : BaseEntity
    {
        return Set<T>();
    }

    public async Task<T?> GetByIdAsync<T>(Guid id) where T : BaseEntity
    {
        return await GetDbSet<T>()
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
    }

    public async Task<IEnumerable<T>> GetAllAsync<T>() where T : BaseEntity
    {
        return await GetDbSet<T>()
            .Where(e => !e.IsDeleted)
            .ToListAsync();
    }

    public async Task<T> AddAsync<T>(T entity) where T : BaseEntity
    {
        entity.CreatedAt = DateTime.UtcNow;
        await GetDbSet<T>().AddAsync(entity);
        await SaveChangesAsync();
        return entity;
    }

    public async Task<T> UpdateAsync<T>(T entity) where T : BaseEntity
    {
        var existing = await GetByIdAsync<T>(entity.Id);
        if (existing == null)
            throw new KeyNotFoundException($"Entity of type {typeof(T).Name} with id {entity.Id} not found");

        entity.UpdatedAt = DateTime.UtcNow;
        GetDbSet<T>().Update(entity);
        await SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync<T>(Guid id) where T : BaseEntity
    {
        var entity = await GetByIdAsync<T>(id);
        if (entity == null)
            throw new KeyNotFoundException($"Entity of type {typeof(T).Name} with id {id} not found");

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        await SaveChangesAsync();
    }

    public async Task HardDeleteAsync<T>(Guid id) where T : BaseEntity
    {
        var entity = await GetByIdAsync<T>(id);
        if (entity == null)
            throw new KeyNotFoundException($"Entity of type {typeof(T).Name} with id {id} not found");

        GetDbSet<T>().Remove(entity);
        await SaveChangesAsync();
    }

    public DbSet<Tag> Tags { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Subscriber> Subscribers { get; set; }
    public DbSet<EventInstance> EventInstances { get; set; }
    public DbSet<SubscriberTag> SubscriberTags { get; set; }
    public DbSet<NotificationRecord> NotificationRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure global query filter for soft delete
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var falseConstant = Expression.Constant(false);
                var lambda = Expression.Lambda(Expression.Equal(property, falseConstant), parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }
} 