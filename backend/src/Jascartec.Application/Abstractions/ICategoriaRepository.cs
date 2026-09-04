using Jascartec.Application.Common;
using Jascartec.Domain.Entities;

namespace Jascartec.Application.Abstractions;

public interface ICategoriaRepository : IRepository<Categoria>
{
    Task<bool> ExisteNombreAsync(string nombre, int? excluirId = null, CancellationToken ct = default);
    Task<bool> TieneProductosAsociadosAsync(int categoriaId, CancellationToken ct = default);
}
