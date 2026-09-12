using Jascartec.Application.Common;
using Jascartec.Domain.Entities;

namespace Jascartec.Application.Abstractions;

public interface ICajaSesionRepository : IRepository<CajaSesion>
{
    Task<CajaSesion?> GetAbiertaAsync(CancellationToken ct = default);
    Task<CajaSesion?> GetByFechaAsync(DateOnly fecha, CancellationToken ct = default);
    Task<CajaSesion?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<CajaSesion>> GetAllWithDetailsAsync(CancellationToken ct = default);
}
