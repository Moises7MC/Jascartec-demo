using Jascartec.Application.Abstractions;
using Jascartec.Domain.Entities;
using Jascartec.Domain.Enums;
using Jascartec.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jascartec.Infrastructure.Repositories;

public class EquipoRepository(JascartecDbContext context) : Repository<Equipo>(context), IEquipoRepository
{
    public Task<bool> ExisteImeiAsync(string imei, CancellationToken ct = default) =>
        Set.AnyAsync(e => e.Imei == imei, ct);

    public Task<Equipo?> GetPrimerDisponiblePorProductoAsync(int productoId, IReadOnlyCollection<int> excluidos, CancellationToken ct = default) =>
        Set.Where(e => e.ProductoId == productoId && e.EstadoVenta == EstadoVenta.Disponible && !excluidos.Contains(e.Id))
            .OrderBy(e => e.Id)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<Equipo>> GetDisponiblesPorProductoAsync(int productoId, CancellationToken ct = default) =>
        await Set.Where(e => e.ProductoId == productoId && e.EstadoVenta == EstadoVenta.Disponible).ToListAsync(ct);

    public Task<int> ContarDisponiblesPorProductoAsync(int productoId, CancellationToken ct = default) =>
        Set.CountAsync(e => e.ProductoId == productoId && e.EstadoVenta == EstadoVenta.Disponible, ct);

    public Task<bool> ExisteAlgunoPorProductoAsync(int productoId, CancellationToken ct = default) =>
        Set.AnyAsync(e => e.ProductoId == productoId, ct);
}
