using Jascartec.Application.Common;
using Jascartec.Domain.Entities;

namespace Jascartec.Application.Abstractions;

public interface IEquipoRepository : IRepository<Equipo>
{
    Task<bool> ExisteImeiAsync(string imei, CancellationToken ct = default);

    Task<IReadOnlyList<Equipo>> GetDisponiblesPorProductoAsync(int productoId, CancellationToken ct = default);

    Task<int> ContarDisponiblesPorProductoAsync(int productoId, CancellationToken ct = default);

    Task<bool> ExisteAlgunoPorProductoAsync(int productoId, CancellationToken ct = default);
}
