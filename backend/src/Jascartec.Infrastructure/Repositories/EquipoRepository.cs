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

    public async Task<IReadOnlyList<Equipo>> GetDisponiblesPorProductoAsync(int productoId, CancellationToken ct = default) =>
        await Set.Where(e => e.ProductoId == productoId && e.EstadoVenta == EstadoVenta.Disponible).ToListAsync(ct);

    public Task<int> ContarDisponiblesPorProductoAsync(int productoId, CancellationToken ct = default) =>
        Set.CountAsync(e => e.ProductoId == productoId && e.EstadoVenta == EstadoVenta.Disponible, ct);

    public Task<bool> ExisteAlgunoPorProductoAsync(int productoId, CancellationToken ct = default) =>
        Set.AnyAsync(e => e.ProductoId == productoId, ct);
}
