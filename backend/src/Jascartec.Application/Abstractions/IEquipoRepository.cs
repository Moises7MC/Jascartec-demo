using Jascartec.Application.Common;
using Jascartec.Domain.Entities;

namespace Jascartec.Application.Abstractions;

public interface IEquipoRepository : IRepository<Equipo>
{
    Task<bool> ExisteImeiAsync(string imei, CancellationToken ct = default);

    /// <summary>sucursalId null = de cualquier sucursal (vista general); con valor, solo los
    /// que están físicamente en esa sucursal (para elegir un IMEI al vender ahí).</summary>
    Task<IReadOnlyList<Equipo>> GetDisponiblesPorProductoAsync(int productoId, int? sucursalId = null, CancellationToken ct = default);

    Task<int> ContarDisponiblesPorProductoAsync(int productoId, CancellationToken ct = default);

    /// <summary>Cuántos equipos disponibles de este producto hay en cada sucursal (para el
    /// desglose de stock del catálogo).</summary>
    Task<IReadOnlyDictionary<int, int>> ContarDisponiblesPorProductoAgrupadoPorSucursalAsync(int productoId, CancellationToken ct = default);

    Task<bool> ExisteAlgunoPorProductoAsync(int productoId, CancellationToken ct = default);
}
