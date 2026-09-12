using Jascartec.Application.Abstractions;
using Jascartec.Domain.Entities;
using Jascartec.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jascartec.Infrastructure.Repositories;

public class ProductoRepository(JascartecDbContext context) : Repository<Producto>(context), IProductoRepository
{
    public async Task<IReadOnlyList<Producto>> GetAllWithDetailsAsync(CancellationToken ct = default) =>
        await Set.Include(p => p.Categoria).Include(p => p.Marca).Include(p => p.Proveedor)
            .Include(p => p.Stocks).ThenInclude(s => s.Sucursal)
            .OrderBy(p => p.Modelo).ToListAsync(ct);

    public Task<Producto?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default) =>
        Set.Include(p => p.Categoria).Include(p => p.Marca).Include(p => p.Proveedor)
            .Include(p => p.Stocks).ThenInclude(s => s.Sucursal)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
}
