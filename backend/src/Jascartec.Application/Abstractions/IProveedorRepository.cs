using Jascartec.Application.Common;
using Jascartec.Domain.Entities;

namespace Jascartec.Application.Abstractions;

public interface IProveedorRepository : IRepository<Proveedor>
{
    Task<bool> TieneVentasAsociadasAsync(int proveedorId, CancellationToken ct = default);
}
