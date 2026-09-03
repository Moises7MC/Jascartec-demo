using Jascartec.Application.Common;
using Jascartec.Domain.Entities;

namespace Jascartec.Application.Abstractions;

public interface IClienteRepository : IRepository<Cliente>
{
    Task<bool> ExisteDocumentoAsync(string documento, int? excluirId = null, CancellationToken ct = default);
    Task<bool> TieneVentasAsociadasAsync(int clienteId, CancellationToken ct = default);
}
