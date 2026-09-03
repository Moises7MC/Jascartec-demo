using Jascartec.Application.Dtos;

namespace Jascartec.Application.Services;

public interface IClienteService
{
    Task<IReadOnlyList<ClienteDto>> ListarAsync(CancellationToken ct = default);
    Task<ClienteDto> CrearAsync(GuardarClienteRequest request, CancellationToken ct = default);
    Task<ClienteDto> ActualizarAsync(int id, GuardarClienteRequest request, CancellationToken ct = default);
    Task EliminarAsync(int id, CancellationToken ct = default);
}
