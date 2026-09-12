using Jascartec.Application.Dtos;

namespace Jascartec.Application.Services;

public interface ISucursalService
{
    Task<IReadOnlyList<SucursalDto>> ListarAsync(CancellationToken ct = default);
    Task<SucursalDto> CrearAsync(GuardarSucursalRequest request, CancellationToken ct = default);
    Task<SucursalDto> ActualizarAsync(int id, GuardarSucursalRequest request, CancellationToken ct = default);
}
