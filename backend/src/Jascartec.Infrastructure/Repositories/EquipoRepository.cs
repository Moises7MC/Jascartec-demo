using Jascartec.Application.Abstractions;
using Jascartec.Domain.Entities;
using Jascartec.Domain.Enums;
using Jascartec.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jascartec.Infrastructure.Repositories;

public class EquipoRepository(JascartecDbContext context) : Repository<Equipo>(context), IEquipoRepository
{
    // Un IMEI puede estar guardado como el principal o como el segundo (dual SIM) de otro equipo:
    // hay que buscarlo en las dos columnas para no permitir que se repita entre ellas.
    public Task<bool> ExisteImeiAsync(string imei, CancellationToken ct = default) =>
        Set.AnyAsync(e => e.Imei == imei || e.Imei2 == imei, ct);

    public async Task<IReadOnlyList<Equipo>> GetDisponiblesPorProductoAsync(int productoId, int? sucursalId = null, CancellationToken ct = default)
    {
        var query = Set.Include(e => e.Sucursal).Where(e => e.ProductoId == productoId && e.EstadoVenta == EstadoVenta.Disponible);
        if (sucursalId.HasValue) query = query.Where(e => e.SucursalId == sucursalId.Value);
        return await query.ToListAsync(ct);
    }

    public Task<int> ContarDisponiblesPorProductoAsync(int productoId, CancellationToken ct = default) =>
        Set.CountAsync(e => e.ProductoId == productoId && e.EstadoVenta == EstadoVenta.Disponible, ct);

    public async Task<IReadOnlyDictionary<int, int>> ContarDisponiblesPorProductoAgrupadoPorSucursalAsync(int productoId, CancellationToken ct = default)
    {
        var grupos = await Set
            .Where(e => e.ProductoId == productoId && e.EstadoVenta == EstadoVenta.Disponible)
            .GroupBy(e => e.SucursalId)
            .Select(g => new { SucursalId = g.Key, Cantidad = g.Count() })
            .ToListAsync(ct);
        return grupos.ToDictionary(g => g.SucursalId, g => g.Cantidad);
    }

    public Task<bool> ExisteAlgunoPorProductoAsync(int productoId, CancellationToken ct = default) =>
        Set.AnyAsync(e => e.ProductoId == productoId, ct);
}
