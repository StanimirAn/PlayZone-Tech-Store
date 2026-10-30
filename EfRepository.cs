using Microsoft.EntityFrameworkCore;

namespace SprintOneStore.Data;

public sealed class EfRepository<TEntity> : IRepository<TEntity> where TEntity : class
{
    private readonly StoreDbContext _db;
    private readonly DbSet<TEntity> _set;

    public EfRepository(StoreDbContext db)
    {
        _db = db;
        _set = db.Set<TEntity>();
    }

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _set.AsNoTracking().ToListAsync(cancellationToken);

    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _set.FindAsync([id], cancellationToken).AsTask();

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await _set.AddAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _set.FindAsync([id], cancellationToken);
        if (entity is null) return false;
        _set.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _set.Update(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
