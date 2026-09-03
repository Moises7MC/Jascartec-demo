using Jascartec.Application.Common;
using Jascartec.Domain.Entities;

namespace Jascartec.Application.Abstractions;

public interface IVentaRepository : IRepository<Venta>
{
    Task<bool> ExisteNumBoletaAsync(string numBoleta, CancellationToken ct = default);
    Task<string> GenerarSiguienteNumBoletaAsync(CancellationToken ct = default);

    Task<IReadOnlyList<Venta>> GetAllWithDetailsAsync(CancellationToken ct = default);
    Task<Venta?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
}
