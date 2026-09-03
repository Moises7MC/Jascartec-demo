using Jascartec.Application.Abstractions;
using Jascartec.Domain.Entities;
using Jascartec.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jascartec.Infrastructure.Repositories;

public class UsuarioRepository(JascartecDbContext context) : Repository<Usuario>(context), IUsuarioRepository
{
    public Task<Usuario?> GetByNombreUsuarioAsync(string nombreUsuario, CancellationToken ct = default) =>
        Set.FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario, ct);
}
