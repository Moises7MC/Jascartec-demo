using Jascartec.Application.Dtos;

namespace Jascartec.Application.Services;

public interface IUsuarioService
{
    Task<IReadOnlyList<UsuarioDto>> ListarAsync(CancellationToken ct = default);
    Task<UsuarioDto> CrearAsync(CrearUsuarioRequest request, CancellationToken ct = default);
    Task<UsuarioDto> ActualizarAsync(int id, ActualizarUsuarioRequest request, CancellationToken ct = default);
    Task EliminarAsync(int id, CancellationToken ct = default);
}
