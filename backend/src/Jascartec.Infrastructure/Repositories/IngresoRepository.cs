using Jascartec.Application.Abstractions;
using Jascartec.Domain.Entities;
using Jascartec.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jascartec.Infrastructure.Repositories;

public class IngresoRepository(JascartecDbContext context) : Repository<Ingreso>(context), IIngresoRepository
{
    public async Task<IReadOnlyList<Ingreso>> GetAllWithDetailsAsync(CancellationToken ct = default) =>
        await Set.Include(i => i.Proveedor)
            .Include(i => i.Equipos).ThenInclude(e => e.Producto).ThenInclude(p => p.Marca)
            .OrderByDescending(i => i.Fecha).ToListAsync(ct);

    public Task<Ingreso?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default) =>
        Set.Include(i => i.Proveedor)
            .Include(i => i.Equipos).ThenInclude(e => e.Producto).ThenInclude(p => p.Marca)
            .FirstOrDefaultAsync(i => i.Id == id, ct);
}
