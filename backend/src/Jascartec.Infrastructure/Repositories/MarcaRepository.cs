using Jascartec.Application.Abstractions;
using Jascartec.Domain.Entities;
using Jascartec.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jascartec.Infrastructure.Repositories;

public class MarcaRepository(JascartecDbContext context) : Repository<Marca>(context), IMarcaRepository
{
    public Task<bool> ExisteNombreAsync(string nombre, int? excluirId = null, CancellationToken ct = default) =>
        Set.AnyAsync(m => m.Nombre == nombre && (excluirId == null || m.Id != excluirId), ct);

    public Task<bool> TieneProductosAsociadosAsync(int marcaId, CancellationToken ct = default) =>
        Context.Productos.AnyAsync(p => p.MarcaId == marcaId, ct);
}
