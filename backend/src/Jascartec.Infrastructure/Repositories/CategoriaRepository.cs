using Jascartec.Application.Abstractions;
using Jascartec.Domain.Entities;
using Jascartec.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jascartec.Infrastructure.Repositories;

public class CategoriaRepository(JascartecDbContext context) : Repository<Categoria>(context), ICategoriaRepository
{
    public Task<bool> ExisteNombreAsync(string nombre, int? excluirId = null, CancellationToken ct = default) =>
        Set.AnyAsync(c => c.Nombre == nombre && (excluirId == null || c.Id != excluirId), ct);

    public Task<bool> TieneProductosAsociadosAsync(int categoriaId, CancellationToken ct = default) =>
        Context.Productos.AnyAsync(p => p.CategoriaId == categoriaId, ct);
}
