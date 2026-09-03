using Jascartec.Application.Abstractions;
using Jascartec.Domain.Entities;
using Jascartec.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jascartec.Infrastructure.Repositories;

public class FacturaRepository(JascartecDbContext context) : Repository<Factura>(context), IFacturaRepository
{
    public async Task<IReadOnlyList<Factura>> GetAllWithLetrasAsync(CancellationToken ct = default) =>
        await Set.Include(f => f.Proveedor).Include(f => f.Letras).OrderByDescending(f => f.Fecha).ToListAsync(ct);

    public Task<Factura?> GetByIdWithLetrasAsync(int id, CancellationToken ct = default) =>
        Set.Include(f => f.Proveedor).Include(f => f.Letras).FirstOrDefaultAsync(f => f.Id == id, ct);
}
