using Jascartec.Application.Abstractions;
using Jascartec.Domain.Entities;
using Jascartec.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jascartec.Infrastructure.Repositories;

public class ProveedorRepository(JascartecDbContext context) : Repository<Proveedor>(context), IProveedorRepository
{
    public Task<bool> TieneVentasAsociadasAsync(int proveedorId, CancellationToken ct = default) =>
        Context.Productos.AnyAsync(p => p.ProveedorId == proveedorId, ct);
}
