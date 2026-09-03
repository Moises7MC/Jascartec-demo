using Jascartec.Application.Common;
using Jascartec.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jascartec.Infrastructure.Repositories;

/// <summary>Implementación genérica compartida por todos los repositorios (evita repetir CRUD básico).</summary>
public class Repository<T>(JascartecDbContext context) : IRepository<T> where T : class
{
    protected readonly JascartecDbContext Context = context;
    protected readonly DbSet<T> Set = context.Set<T>();

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken ct = default) => await Set.FindAsync([id], ct);

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default) => await Set.ToListAsync(ct);

    public virtual async Task AddAsync(T entity, CancellationToken ct = default) => await Set.AddAsync(entity, ct);

    public virtual void Update(T entity) => Set.Update(entity);

    public virtual void Remove(T entity) => Set.Remove(entity);
}
