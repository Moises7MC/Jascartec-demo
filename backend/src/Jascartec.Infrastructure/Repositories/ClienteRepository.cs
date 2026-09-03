using Jascartec.Application.Abstractions;
using Jascartec.Domain.Entities;
using Jascartec.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jascartec.Infrastructure.Repositories;

public class ClienteRepository(JascartecDbContext context) : Repository<Cliente>(context), IClienteRepository
{
    public Task<bool> ExisteDocumentoAsync(string documento, int? excluirId = null, CancellationToken ct = default) =>
        Set.AnyAsync(c => c.Documento == documento && (excluirId == null || c.Id != excluirId), ct);

    public Task<bool> TieneVentasAsociadasAsync(int clienteId, CancellationToken ct = default) =>
        Context.Ventas.AnyAsync(v => v.ClienteId == clienteId, ct);
}
