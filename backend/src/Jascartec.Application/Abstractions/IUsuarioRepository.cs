using Jascartec.Application.Common;
using Jascartec.Domain.Entities;

namespace Jascartec.Application.Abstractions;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario?> GetByNombreUsuarioAsync(string nombreUsuario, CancellationToken ct = default);
}
