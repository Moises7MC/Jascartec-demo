namespace Jascartec.Application.Common;

/// <summary>
/// Operaciones CRUD genéricas. Los repositorios específicos (ej. IClienteRepository)
/// heredan de esta interfaz y solo agregan lo que necesitan de más (ISP: nadie se ve
/// obligado a implementar métodos que no usa).
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
}
