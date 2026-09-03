using Jascartec.Application.Common;
using Jascartec.Domain.Entities;

namespace Jascartec.Application.Abstractions;

public interface IMarcaRepository : IRepository<Marca>
{
    Task<bool> ExisteNombreAsync(string nombre, int? excluirId = null, CancellationToken ct = default);
    Task<bool> TieneProductosAsociadosAsync(int marcaId, CancellationToken ct = default);
}
