using Jascartec.Application.Common;
using Jascartec.Domain.Entities;

namespace Jascartec.Application.Abstractions;

public interface IProductoRepository : IRepository<Producto>
{
    /// <summary>Trae los productos con Marca y Proveedor ya cargados (evita N+1 al listar).</summary>
    Task<IReadOnlyList<Producto>> GetAllWithDetailsAsync(CancellationToken ct = default);
    Task<Producto?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
}
