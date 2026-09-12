using Jascartec.Application.Abstractions;
using Jascartec.Domain.Entities;
using Jascartec.Domain.Enums;
using Jascartec.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jascartec.Infrastructure.Repositories;

public class CajaSesionRepository(JascartecDbContext context) : Repository<CajaSesion>(context), ICajaSesionRepository
{
    private IQueryable<CajaSesion> ConDetalles() => Set
        .Include(c => c.UsuarioApertura)
        .Include(c => c.UsuarioCierre)
        .Include(c => c.Sucursal)
        .Include(c => c.Movimientos).ThenInclude(m => m.Usuario);

    public Task<CajaSesion?> GetAbiertaAsync(int sucursalId, CancellationToken ct = default) =>
        ConDetalles().FirstOrDefaultAsync(c => c.Estado == EstadoCajaSesion.Abierta && c.SucursalId == sucursalId, ct);

    public Task<CajaSesion?> GetByFechaAsync(DateOnly fecha, int sucursalId, CancellationToken ct = default) =>
        Set.FirstOrDefaultAsync(c => c.Fecha == fecha && c.SucursalId == sucursalId, ct);

    public Task<CajaSesion?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default) =>
        ConDetalles().FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<CajaSesion>> GetAllWithDetailsAsync(CancellationToken ct = default) =>
        await ConDetalles().OrderByDescending(c => c.Fecha).ToListAsync(ct);
}
