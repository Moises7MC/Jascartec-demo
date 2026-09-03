using Jascartec.Application.Dtos;

namespace Jascartec.Application.Services;

public interface IProveedorService
{
    Task<IReadOnlyList<ProveedorDto>> ListarAsync(CancellationToken ct = default);
    Task<ProveedorDto> CrearAsync(GuardarProveedorRequest request, CancellationToken ct = default);
    Task<ProveedorDto> ActualizarAsync(int id, GuardarProveedorRequest request, CancellationToken ct = default);
    Task EliminarAsync(int id, CancellationToken ct = default);
}
