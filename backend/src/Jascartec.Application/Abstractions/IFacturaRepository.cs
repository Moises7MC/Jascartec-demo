using Jascartec.Application.Common;
using Jascartec.Domain.Entities;

namespace Jascartec.Application.Abstractions;

public interface IFacturaRepository : IRepository<Factura>
{
    Task<IReadOnlyList<Factura>> GetAllWithLetrasAsync(CancellationToken ct = default);
    Task<Factura?> GetByIdWithLetrasAsync(int id, CancellationToken ct = default);
}
