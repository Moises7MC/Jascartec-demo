using Jascartec.Application.Common;
using Jascartec.Domain.Entities;

namespace Jascartec.Application.Abstractions;

public interface IIngresoRepository : IRepository<Ingreso>
{
    Task<IReadOnlyList<Ingreso>> GetAllWithDetailsAsync(CancellationToken ct = default);
    Task<Ingreso?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
}
